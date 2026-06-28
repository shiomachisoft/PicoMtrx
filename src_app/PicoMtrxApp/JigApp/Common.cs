// Copyright © 2024 Shiomachi Software. All rights reserved.
using System.Drawing;
using System.Windows.Forms;

namespace JigApp
{
    /// <summary>
    /// UI操作のクラス
    /// </summary>
    public static class UI
    {
        /// <summary>
        /// 接続状態表示用（切断・異常時）の'赤'を取得する
        /// </summary>
        public static Color MonRed { get; } = Color.FromArgb(255, 150, 150);

        /// <summary>
        /// 接続状態表示用（接続・正常時）の'緑'を取得する
        /// </summary>
        public static Color MonGreen { get; } = Color.FromArgb(150, 255, 150);

        /// <summary>
        /// エラーログを出力し、メッセージボックスを表示する
        /// </summary>
        public static void ShowErrMsg(Form frm, string strMsg)
        {
            FormMain.Inst.AppendAppLogText(true, strMsg);
            MessageBox.Show(frm, strMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
