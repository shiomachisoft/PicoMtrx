// Copyright © 2024 Shiomachi Software. All rights reserved.
using System;
using System.Windows.Forms;
using JigLib;

namespace JigApp
{
    public static class Program
    {
        /// <summary>
        /// 治具コマンドのインスタンス
        /// </summary>
        public static JigCmd PrpJigCmd { get; set; } = new JigSerial();

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}
