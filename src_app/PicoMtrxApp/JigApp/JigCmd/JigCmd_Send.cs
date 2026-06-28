// Copyright © 2024 Shiomachi Software. All rights reserved.
using System;
using System.Collections.Generic;

namespace JigLib
{
    public abstract partial class JigCmd
    {
        /// <summary>
        /// 「FW情報取得」コマンドの要求を送信
        /// </summary>
        public string SendCmd_GetFwInfo(out string strMakerName, out string strFwName, out string strFwVer, out string strBoardId)
        {
            int offset = 0;
            byte[] aReqData = null;
            byte[] aResData = null;
            UInt32 fwVer;
            string strErrMsg;

            strMakerName = null;
            strFwName = null;
            strFwVer = null;
            strBoardId = null;

            strErrMsg = SendCmd(E_FRM_CMD.GET_FW_INFO, aReqData, out aResData);
            if (strErrMsg == null)
            {
                if (aResData == null || aResData.Length < 44)
                {
                    return "Invalid firmware information data size received.";
                }

                strMakerName = string.Empty;
                for (int i = 0; i < 16; i++)
                {
                    if (aResData[i] != '\0')
                    {
                        strMakerName += (char)aResData[i];
                    }
                }
                offset += 16;

                strFwName = string.Empty;
                for (int i = 0; i < 16; i++)
                {
                    if (aResData[offset + i] != '\0')
                    {
                        strFwName += (char)aResData[offset + i];
                    }
                }
                offset += 16;

                fwVer = BitConverter.ToUInt32(aResData, offset);
                offset += 4;
                strFwVer = fwVer.ToString("X8");

                strBoardId = string.Empty;
                for (int i = 0; i < 8; i++)
                {
                    strBoardId += aResData[offset + i].ToString("X2");
                }
            }

            return strErrMsg;
        }

        /// <summary>
        /// 「マトリクスデータクリア」コマンドの要求を送信
        /// </summary>
        public string SendCmd_ClearMatrix()
        {
            byte[] aReqData = null;
            byte[] aResData = null;
            string strErrMsg;

            strErrMsg = SendCmd(E_FRM_CMD.CLEAR_MATRIX, aReqData, out aResData);

            return strErrMsg;
        }

        /// <summary>
        /// 「マトリクスデータ更新」コマンドの要求を送信
        /// </summary>
        public string SendCmd_UpdateMatrix(byte[] sendMatrixData)
        {
            byte[] aReqData = sendMatrixData;
            byte[] aResData = null;
            string strErrMsg;

            strErrMsg = SendCmd(E_FRM_CMD.UPDATE_MATRIX, aReqData, out aResData);

            return strErrMsg;
        }

        /// <summary>
        /// 要求フレームを送信
        /// </summary>
        private string SendCmd(E_FRM_CMD eCmd, byte[] aReqData, out byte[] aResData, int resTimeout = FRM_RES_TIMEOUT)
        {
            byte[] aReqFrm;            // 要求フレーム
            string strErrMsg = null;   // エラーメッセージ
            ST_FRM_REQ_FRAME stReqFrm; // 要求フレーム
            ST_FRM_RES_FRAME stResFrm; // 応答フレーム

            lock (_lockSend) // 送信～応答待ち中は、次の送信をしないようにするためのロック
            {
                aResData = null;

                // 応答フレーム受信キューを空にする
                PrpResEvent.Reset();
                while (true == PrpResFrmQue.TryTake(out stResFrm)) { }

                // [要求フレームを作成]
                stReqFrm.header = E_FRM_HEADER.REQ; // ヘッダ
                stReqFrm.seqNo = _seqNo++;          // シーケンス番号
                stReqFrm.cmd = eCmd;                // コマンド
                if (aReqData == null) // データ部が空の場合
                {
                    stReqFrm.dataSize = 0;  // データサイズ
                    stReqFrm.aData = null;  // データ
                }
                else // データ部が空ではない場合
                {
                    stReqFrm.dataSize = (UInt16)aReqData.Length; // データサイズ
                    stReqFrm.aData = aReqData;                   // データ
                }
                // チェックサム計算前の要求フレームのbyte型配列を取得
                stReqFrm.checksum = 0;
                aReqFrm = ConvertReqFrameStructToByteArray(stReqFrm);
                // チェックサムを計算
                stReqFrm.checksum = CalcChecksum(aReqFrm, aReqFrm.Length - 2);

                // [要求フレームを送信]
                // チェックサム計算後の要求フレームのbyte型配列を取得
                aReqFrm = ConvertReqFrameStructToByteArray(stReqFrm);
                // 要求フレームを送信
                strErrMsg = Send(aReqFrm);
                if (strErrMsg != null)
                {
                    // 送信失敗
                    _isConnected = false; // 切断しているとみなす
                    goto End;
                }

                // [応答フレーム受信イベント発生待ち]
                if (!PrpResEvent.WaitOne(resTimeout))
                {
                    strErrMsg = "Response frame reception timeout.";
                    _isConnected = false; // 切断しているとみなす
                    goto End;
                }

                // [応答フレーム受信キューから応答フレームを取り出す]
                if (PrpResFrmQue.TryTake(out stResFrm))
                {
                    if (stResFrm.seqNo != stReqFrm.seqNo)
                    {
                        strErrMsg = "The sequence number in the response does not match the request.";
                        goto End;
                    }
                    if (stResFrm.cmd != stReqFrm.cmd)
                    {
                        strErrMsg = "The command being responded to does not match the request.";
                        goto End;
                    }

                    if (stResFrm.errCode == E_FRM_ERRCODE.SUCCESS)
                    {
                        aResData = new byte[stResFrm.aData.Length];
                        Array.Copy(stResFrm.aData, aResData, aResData.Length);
                    }
                    else
                    {
                        strErrMsg = ConvertErrCodeInResFrameToMsg(stResFrm.errCode);
                        goto End;
                    }
                }
                else
                {
                    strErrMsg = STR_MSG_WAIT_RES_CANCEL;
                    goto End;
                }
        End:

                return strErrMsg;
            }
        }

        /// <summary>
        /// 要求フレーム構造体をbyte型配列へ変換する
        /// </summary>
        private byte[] ConvertReqFrameStructToByteArray(ST_FRM_REQ_FRAME stReqFrm)
        {
            List<byte[]> lst = new List<byte[]>(); // byte型配列のリスト

            // 要求フレーム構造体の各フィールドをbyte型配列に変換してリストに追加
            lst.Add(new byte[1] { (byte)stReqFrm.header });
            lst.Add(BitConverter.GetBytes(stReqFrm.seqNo));
            lst.Add(BitConverter.GetBytes((UInt16)stReqFrm.cmd));
            lst.Add(BitConverter.GetBytes(stReqFrm.dataSize));
            lst.Add(stReqFrm.aData);
            lst.Add(BitConverter.GetBytes(stReqFrm.checksum));

            // リストを1つのbyte型配列に結合して返す
            return CombineByteArray(lst);
        }

        /// <summary>
        /// 引数のbyte型配列のリストを1つのbyte型配列に結合して返す
        /// </summary>
        private byte[] CombineByteArray(List<byte[]> lst)
        {
            // 返却するbyte型配列のサイズを求める
            int size = 0;
            foreach (byte[] ary in lst)
            {
                if (ary != null)
                {
                    size += ary.Length;
                }
            }

            // 引数のbyte型配列のリストを1つのbyte型配列に結合する
            int offset = 0;
            byte[] buf = new byte[size];
            foreach (byte[] ary in lst)
            {
                if (ary != null)
                {
                    Buffer.BlockCopy(ary, 0, buf, offset, ary.Length);
                    offset += ary.Length;
                }
            }

            return buf;
        }
    }
}
