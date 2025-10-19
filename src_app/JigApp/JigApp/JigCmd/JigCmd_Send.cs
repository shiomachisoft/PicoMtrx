// Copyright © 2024 Shiomachi Software. All rights reserved.
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// 「FWエラー情報取得」コマンドの要求を送信
        /// </summary>
        public string SendCmd_GetFwError(ref List<string> lstErrMsg)
        {
            byte[] aReqData = null;
            byte[] aResData = null;
            UInt32 errBits = 0;
            string strErrMsg;

            strErrMsg = SendCmd(E_FRM_CMD.GET_FW_ERR, aReqData, out aResData);
            if (strErrMsg == null)
            {
                errBits = BitConverter.ToUInt32(aResData, 0);
                lstErrMsg.Clear();
                for (int i = 0; i < 32/*FW_ERR_MSG_ARY.Length*/; i++)
                {
                    if ((errBits & (1 << i)) != 0)
                    {
                        if (i < FW_ERR_MSG_ARY.Length)
                        {
                            lstErrMsg.Add(FW_ERR_MSG_ARY[i]);
                        }
                        else
                        {
                            lstErrMsg.Add("Undefined error");
                        }
                    } 
                }
            }

            return strErrMsg;
        }

        /// <summary>
        /// 「FWエラークリア」コマンドの要求を送信
        /// </summary>
        public string SendCmd_ClearFwError()
        {
            byte[] aReqData = null;
            byte[] aResData = null;
            string strErrMsg;

            strErrMsg = SendCmd(E_FRM_CMD.CLEAR_FW_ERR, aReqData, out aResData);

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

                // [応答無しのコマンドの場合]
                /*
                switch (eCmd)
                {
                    case E_FRM_CMD.SEND_UART:
                        goto End;
                    default:
                        break;
                }
                */

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

        /// <summary>
        /// IPアドレスの文字列をbyte型の配列に変換する
        /// </summary>
        /// <remarks>
        /// セパレータはドット。
        /// </remarks>
        private string ConvertIpAddrStringToByteArray(string strText, out byte[] aVal)
        {
            char[] aSeparator = { '.' }; // セパレータ
            string strErrMsg = ConvertStringToValArray(strText, aSeparator, 10, out aVal);
            bool isErr = false;

            if (strErrMsg == null)
            {
                if (aVal.Length != 4)
                {
                    isErr = true;
                }
            }
            else
            {
                isErr = true;
            }

            if (isErr)
            {
                strErrMsg = "Invalid parameter. (IP address)";
            }

            return strErrMsg;
        }

        /// <summary>
        /// 文字列をbyte型の配列に変換する
        /// </summary>
        private string ConvertStringToValArray(string strText, char[] aSeparator, int baseNumber, out byte[] aVal)
        {
            string[] astrSplit; // 分割後の文字列
            string strErrMsg = null;

            // 文字列をセパレータで分割
            strText = strText.Replace("\r\n", "\r");
            astrSplit = strText.Split(aSeparator);

            // [文字列をbyte型の配列に変換]
            // 要素数が分割された文字列の数であるbyte型配列を用意
            aVal = new byte[astrSplit.Count()];
            // 分割された文字列の数だけ、文字列をbyte型に変換
            for (int i = 0; i < astrSplit.Count(); i++)
            {
                try
                {
                    aVal[i] = Convert.ToByte(astrSplit[i], baseNumber);
                }
                catch (Exception ex)
                {
                    strErrMsg = ex.Message;
                }
            }

            return strErrMsg;
        }

        /// <summary>
        /// char型の配列をbyte型の配列に変換する
        /// </summary>
        private byte[] ConvertCharAryToByteAry(char[] achArray)
        {
            byte[] abyArray = new byte[achArray.Length];

            for (int i = 0; i < achArray.Length; i++)
            {
                abyArray[i] = (byte)achArray[i];
            }

            return abyArray;
        }
    }
}
