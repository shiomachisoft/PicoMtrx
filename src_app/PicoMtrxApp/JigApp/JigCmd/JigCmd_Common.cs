// Copyright © 2024 Shiomachi Software. All rights reserved.
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace JigLib
{
    public abstract partial class JigCmd
    {
        /// <summary>
        /// 送信コマンドの応答待ちをキャンセルした時のメッセージ
        /// </summary>
        public const string STR_MSG_WAIT_RES_CANCEL = "Waiting for a response to send a command has been canceled.";

        /// <summary>
        /// フレーム中のデータ部の最大サイズ
        /// </summary>
        /// <remarks>
        /// FW側のFRM_DATA_MAX_SIZEの値に合わせる
        /// </remarks>
        protected const int FRM_DATA_MAX_SIZE = 1024;
        /// <summary>
        /// 応答フレームの最大サイズ
        /// </summary>
        /// <remarks>
        /// データ最大サイズ 1024バイト ＋ 固定ヘッダ・フッタ領域等 11バイト
        /// </remarks>
        protected const int FRM_RES_SIZE = FRM_DATA_MAX_SIZE + 11;
        /// <summary>
        /// 受信タスクの終了待ちタイムアウト時間(ms)
        /// </summary>
        protected const int RECV_TASK_END_TIMEOUT = 20000;
        /// <summary>
        /// 応答フレーム受信タイムアウト(ms)
        /// </summary>
        protected const int FRM_RES_TIMEOUT = 10000;

        /// <summary>
        /// フレームエンドタイムアウト(ms)
        /// </summary>
        /// <remarks>
        /// 受信フレーム(応答フレーム)のヘッダを受信後、フレームエンドタイムアウトの時間が経過してもそのフレームの末端を受信しなかった場合、そのフレームは破棄する
        /// </remarks>
        private const int FRM_END_TIMEOUT = 1000;
        /// <summary>
        /// Recv()のwhile文のディレイ(ms)
        /// </summary>
        private const int RECV_DELAY = 50;

        /// <summary>
        /// フレーム中のヘッダ部の定義
        /// </summary>
        protected enum E_FRM_HEADER : byte
        {
            /// <summary>
            /// 要求フレーム
            /// </summary>
            REQ = 0xA0,
            /// <summary>
            /// 応答フレーム
            /// </summary>
            RES
        }

        /// <summary>
        /// フレーム中のコマンド部の定義
        /// </summary>
        protected enum E_FRM_CMD : UInt16
        {
            /// <summary>
            /// FW情報取得
            /// </summary>
            GET_FW_INFO = 0x0001,
            /// <summary>
            /// マトリクスデータクリア
            /// </summary>
            CLEAR_MATRIX,
            /// <summary>
            /// マトリクスデータ更新
            /// </summary>
            UPDATE_MATRIX,
        }

        /// <summary>
        /// 応答フレーム中のエラーコード部の定義
        /// </summary>
        protected enum E_FRM_ERRCODE : UInt16
        {
            /// <summary>
            /// 成功
            /// </summary>
            SUCCESS = 0x0000,
            /// <summary>
            /// 要求中のデータ部のサイズが不正
            /// </summary>
            DATA_SIZE,
            /// <summary>
            /// 要求中の引数が不正
            /// </summary>
            PARAM,
            /// <summary>
            /// バッファに空きがないので要求データを破棄した
            /// </summary>
            BUF_NOT_ENOUGH
        }

        /// <summary>
        /// 要求フレーム構造体
        /// </summary>
        protected struct ST_FRM_REQ_FRAME
        {
            /// <summary>
            /// ヘッダ(1バイト)
            /// </summary>
            public E_FRM_HEADER header;
            /// <summary>
            /// シーケンス番号(2バイト)
            /// </summary>
            public UInt16 seqNo;
            /// <summary>
            /// コマンド(2バイト)
            /// </summary>
            public E_FRM_CMD cmd;
            /// <summary>
            /// データサイズ(2バイト)
            /// </summary>
            public UInt16 dataSize;
            /// <summary>
            /// データ
            /// </summary>
            public byte[] aData;
            /// <summary>
            /// チェックサム(2バイト)
            /// </summary>
            public UInt16 checksum;
        }

        /// <summary>
        /// 応答フレーム構造体
        /// </summary>
        protected struct ST_FRM_RES_FRAME
        {
            /// <summary>
            /// ヘッダ(1バイト)
            /// </summary>
            public E_FRM_HEADER header;
            /// <summary>
            /// シーケンス番号(2バイト)
            /// </summary>
            public UInt16 seqNo;
            /// <summary>
            /// コマンド(2バイト)
            /// </summary>
            public E_FRM_CMD cmd;
            /// <summary>
            /// エラーコード(2バイト)
            /// </summary>
            public E_FRM_ERRCODE errCode;
            /// <summary>
            /// データサイズ(2バイト)
            /// </summary>
            public UInt16 dataSize;
            /// <summary>
            /// データ
            /// </summary>
            public byte[] aData;
            /// <summary>
            /// チェックサム(2バイト)
            /// </summary>
            public UInt16 checksum;
        }

        /// <summary>
        /// 接続済みか否か
        /// </summary>
        protected volatile bool _isConnected = false;
        /// <summary>
        /// 応答フレーム受信イベント
        /// </summary>
        protected ManualResetEvent PrpResEvent { get; set; } = new ManualResetEvent(false);
        /// <summary>
        /// 応答フレームのキュー
        /// </summary>
        protected BlockingCollection<ST_FRM_RES_FRAME> PrpResFrmQue { get; set; } = new BlockingCollection<ST_FRM_RES_FRAME>(1);
        /// <summary>
        /// COMポートのアクセスを排他するためのロック用オブジェクト
        /// </summary>
        protected object _lockPort = new object();
        /// <summary>
        /// 送信～応答待ち中は、次の送信をしないようにするためのロック用オブジェクト
        /// </summary>
        protected object _lockSend = new object();
        /// <summary>
        /// 受信処理に関するリソースを排他するロック用オブジェクト
        /// </summary>
        protected object _lockRecv = new object();
        /// <summary>
        /// 切断処理を実行中か否か
        /// </summary>
        protected volatile bool _isDisconnecting = false;

        /// <summary>
        /// シーケンス番号
        /// </summary>
        private UInt16 _seqNo = 0;
        /// <summary>
        /// 受信フレーム(応答フレーム)の受信サイズ
        /// </summary>
        private int _recvSize = 0;
        /// <summary>
        /// 受信フレーム(応答フレーム)のバッファ
        /// </summary>
        private byte[] _bufRecvFrm = new byte[FRM_RES_SIZE];
        /// <summary>
        /// フレームエンドタイムアウト用タイマー
        /// </summary>
        private Timer _timerFrameEnd = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public JigCmd()
        {
            // フレームエンドタイムアウトのコールバックを登録
            _timerFrameEnd = new Timer(FrameEndTimeoutCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// チェックサムを計算
        /// </summary>
        private UInt16 CalcChecksum(byte[] buf, int size)
        {
            int i;
            UInt16 checksum = 0;

            for (i = 0; i < size; i++)
            {
                checksum += buf[i];
            }

            return checksum;
        }
    }
}
