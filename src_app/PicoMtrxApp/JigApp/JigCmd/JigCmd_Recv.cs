// Copyright © 2024 Shiomachi Software. All rights reserved.
using System;
using System.Threading;

namespace JigLib
{
    public abstract partial class JigCmd
    {
        /// <summary>
        /// 受信データを取り出し・解析して応答フレームを作成
        /// </summary>
        protected void Recv()
        {
            int dataSize = 0;   // 受信フレーム中のデータサイズ部
            byte data;          // シリアル受信データ(1byte)
            UInt16 expected;    // チェックサムの期待値

            DiscardRecvFrame(); // 初期化

            while (!_isDisconnecting)
            {
                bool hasData = false;

                lock (_lockRecv) // 受信処理に関するリソースを排他する
                {
                    try
                    {
                        if (true == HasRecvData()) // 受信データが存在する場合
                        {
                            // 受信データを1byte取り出す
                            if (ReadByte(out data))
                            {
                                hasData = true;
                                // [受信データから応答フレームを作成する]

                                if (_recvSize == 0) // 受信データ = ヘッダ の場合(ヘッダはまだ格納していない場合)
                                {
                                    if (data == (byte)E_FRM_HEADER.RES) // 応答
                                    {
                                        // 受信フレームのバッファをゼロ充填
                                        Array.Clear(_bufRecvFrm, 0, _bufRecvFrm.Length);
                                        // ヘッダを格納
                                        _bufRecvFrm[_recvSize++] = data;
                                        // フレームエンドタイムアウト用タイマーを開始
                                        _timerFrameEnd.Change(FRM_END_TIMEOUT, Timeout.Infinite);
                                    }
                                    else // 不正なフレームヘッダ
                                    {
                                        // 受信フレーム破棄
                                        DiscardRecvFrame();
                                    }
                                }
                                else // ヘッダは格納済みの場合
                                {
                                    if (_recvSize <= 8) // 受信データ = シーケンス番号/コマンド/エラーコード/データサイズ の場合
                                    {
                                        // シーケンス番号/コマンド/エラーコード/データサイズを格納
                                        _bufRecvFrm[_recvSize++] = data;
                                        if (_recvSize == 9) // データサイズは格納済みの場合
                                        {
                                            // データサイズが最大値を超えてないかをチェック
                                            dataSize = _bufRecvFrm[8] << 8 | _bufRecvFrm[7];
                                            if (dataSize > FRM_DATA_MAX_SIZE) // データサイズが最大値を超えている場合
                                            {
                                                // 受信フレーム破棄
                                                DiscardRecvFrame();
                                            }
                                        }
                                    }
                                    else if (_recvSize < (9 + dataSize + 2)) // 受信データ = データ/チェックサム の場合
                                    {
                                        if (_recvSize >= _bufRecvFrm.Length)
                                        {
                                            DiscardRecvFrame();
                                            continue;
                                        }
                                        // データ/チェックサムを格納
                                        _bufRecvFrm[_recvSize++] = data;
                                        if (_recvSize == (9 + dataSize + 2)) // チェックサムは格納済みの場合
                                        {
                                            // チェックサム検査
                                            expected = (UInt16)(((UInt16)_bufRecvFrm[_recvSize - 1]) << 8 | (UInt16)_bufRecvFrm[_recvSize - 2]);
                                            if (TestChecksum(_bufRecvFrm, _recvSize - 2, expected)) // チェックサム検査がOKの場合
                                            {
                                                // 応答フレームをキューイング
                                                ST_FRM_RES_FRAME stResFrm = ConvertByteArrayToResFrameStruct(_bufRecvFrm);
                                                if (PrpResFrmQue.TryAdd(stResFrm))
                                                {
                                                    // 応答フレーム受信をイベント通知
                                                    PrpResEvent.Set();
                                                }
                                            }
                                            // 受信フレーム解析完了
                                            DiscardRecvFrame();
                                        }
                                    }
                                    else
                                    {
                                        // 無処理
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception) // ポートのエラーが発生した場合
                    {
                    }
                }

                if (!hasData)
                {
                    Thread.Sleep(RECV_DELAY);
                }
            }
        }

        /// <summary>
        /// byte型配列を応答フレーム構造体に変換して返す
        /// </summary>
        private ST_FRM_RES_FRAME ConvertByteArrayToResFrameStruct(byte[] buf)
        {
            ST_FRM_RES_FRAME stResFrm;

            stResFrm.header = (E_FRM_HEADER)buf[0];
            stResFrm.seqNo = (UInt16)(((UInt16)buf[2]) << 8 | (UInt16)buf[1]);
            stResFrm.cmd = (E_FRM_CMD)(((UInt16)buf[4]) << 8 | (UInt16)buf[3]);
            stResFrm.errCode = (E_FRM_ERRCODE)(((UInt16)buf[6]) << 8 | (UInt16)buf[5]);
            stResFrm.dataSize = (UInt16)(((UInt16)buf[8]) << 8 | (UInt16)buf[7]);
            stResFrm.aData = new byte[stResFrm.dataSize];
            Buffer.BlockCopy(buf, 9, stResFrm.aData, 0, stResFrm.dataSize);
            int offset = 9 + stResFrm.dataSize;
            stResFrm.checksum = (UInt16)(((UInt16)buf[offset + 1]) << 8 | (UInt16)buf[offset]);

            return stResFrm;
        }

        /// <summary>
        /// チェックサム検査
        /// </summary>
        private bool TestChecksum(byte[] buf, int size, UInt16 expected)
        {
            UInt16 checksum = 0;
            bool bRet = false;

            checksum = CalcChecksum(buf, size);
            if (checksum == expected)
            {
                bRet = true;
            }

            return bRet;
        }

        /// <summary>
        /// 応答フレームのヘッダを受信後、フレームエンドタイムアウトの時間が経過してもそのフレームの末端を受信しなかった場合、そのフレームは破棄する
        /// </summary>
        private void FrameEndTimeoutCallback(object state)
        {
            lock (_lockRecv) // 受信処理に関するリソースを排他する
            {
                // 受信フレーム破棄
                DiscardRecvFrame();
            }
        }

        /// <summary>
        /// 受信フレームを破棄、または解析が完了したため初期化する
        /// </summary>
        private void DiscardRecvFrame()
        {
            // 受信サイズ = 0
            _recvSize = 0;
            // フレームエンドタイムアウト用タイマーを停止
            _timerFrameEnd.Change(Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// 応答フレーム中のエラーコードをエラーメッセージに変換して返す
        /// </summary>
        private string ConvertErrCodeInResFrameToMsg(E_FRM_ERRCODE errCode)
        {
            string strErrMsg = null;

            // FWのソースのE_FRM_ERRに合わせる
            switch (errCode)
            {
                case E_FRM_ERRCODE.SUCCESS:
                    break;
                case E_FRM_ERRCODE.DATA_SIZE:
                    strErrMsg = "A failure response was received from the microcontroller. (The size of the data part being requested is invalid)";
                    break;
                case E_FRM_ERRCODE.PARAM:
                    strErrMsg = "A failure response was received from the microcontroller. (The argument in the request is invalid)";
                    break;
                case E_FRM_ERRCODE.BUF_NOT_ENOUGH:
                    strErrMsg = "A failure response was received from the microcontroller. (There is no space in the buffer)";
                    break;
                default:
                    strErrMsg = "A failure response was received from the microcontroller. (undefined error code)";
                    break;
            }

            return strErrMsg;
        }
    }
}
