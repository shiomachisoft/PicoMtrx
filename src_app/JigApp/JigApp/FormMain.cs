// Copyright © 2024 Shiomachi Software. All rights reserved.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO.Ports;
using System.Diagnostics;
using System.Reflection;
using JigLib;

namespace JigApp
{
    public partial class FormMain : Form
    {
        private const string STR_NOT_DISPLAYED = "---";
        private const string STR_BTN_CONNECT = "connect";
        private const string STR_BTN_DISCONNECT = "disconnect";
        private const string STR_LBL_CONNECT = "connected";
        private const string STR_LBL_DISCONNECT = "disconnected";

        /// <summary>
        /// マイコンが再起動するのを待つ時間(ms)
        /// </summary>
        private const int REBOOT_WAIT = 5000;
        /// <summary>
        /// どれくらいの時間の間、再接続のリトライを行うか(秒)
        /// </summary>
        private const int RECONNECT_TIME = 15;

        /// <summary>
        /// 自分のインスタンス
        /// </summary>
        public static FormMain Inst { get; set; } = null;

        /// <summary>
        /// アプリ名
        /// </summary>
        private string _strAppName = null;
        /// MTRXフォーム
        /// </summary>
        private FormMtrx _formMtrx = null;
        /// <summary>
        /// 子フォーム表示ボタンのリスト
        /// </summary>
        private List<Button> _lstButton = new List<Button>();
        /// <summary>
        /// FWエラーメッセージのリスト
        /// </summary>
        private List<string> _lstFwErrMsg = new List<string>();
        /// <summary>
        /// モニタ用タスク
        /// </summary>
        private Task<string> _tskMon = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FormMain()
        {
            InitializeComponent();
            // 自分のインスタンスを保存
            Inst = this;
            // ボタンのリストを登録
            _lstButton.Add(button_Mtrx);
            _lstButton.Add(button_ClearFwErr);
        }

        /// <summary>
        /// フォームのロード時
        /// </summary>
        private void FormMain_Load(object sender, EventArgs e)
        {
            // アプリ名を表示
            _strAppName = Process.GetCurrentProcess().ProcessName;
            label_AppName.Text = _strAppName;
            // タイトルを表示
            this.Text = _strAppName + " - " + "Monitor stopped";
            // アプリのバージョンを表示
            FileVersionInfo verInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            label_AppVer.Text = verInfo.FileVersion;
            // COMポート名の一覧をコンボボックスに追加
            AddSerialPortToList();
            // 接続状態ラベルの色を設定
            label_ConnectStatus.BackColor = UI.MonRed;
            // 子フォーム表示ボタンを無効に設定
            EnableFormButton(false);
        }

        /// <summary>
        /// COMポート名の一覧をコンボボックスに追加
        /// </summary>
        private void AddSerialPortToList()
        {
            string[] astrPortName;

            // COMポート名一覧を取得
            astrPortName = SerialPort.GetPortNames();
            Array.Sort(astrPortName); // ポート名の昇順にソート

            // ポート名一覧をコンボボックスに追加
            for (int i = 0; i < astrPortName.Length; i++)
            {
                comboBox_Port.Items.Add(astrPortName[i]);
            }

            if (comboBox_Port.Items.Count > 0) // コンボボックスのアイテム数が0より大きい場合
            {
                comboBox_Port.SelectedIndex = 0; // 先頭のアイテムを選択
            }
        }

        /// <summary>
        /// フォームを閉じる時
        /// </summary>
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 切断する
            Program.PrpJigCmd.Disconnect();
        }

        /// <summary>
        /// タイマーコールバック
        /// </summary>
        private void timer_Tick(object sender, EventArgs e)
        {
            // 接続状態とFWエラーのモニタ
            Monitor();
        }

        /// <summary>
        /// 接続状態とFWエラーのモニタ
        /// </summary>
        private async void Monitor()
        {
            string strFwErrMsg;
            string strErrMsg;

            // [接続状態のモニタ]
            if ((!Program.PrpJigCmd.IsConnected()) && (label_ConnectStatus.Text == STR_LBL_CONNECT))
            {
                // マイコンとの接続が切断された場合
                AppendAppLogText(true, "Connection status is abnormal.");
                // 再接続する
                Reconnect();
            }

            // [FWエラーのモニタ]
            if (true == Program.PrpJigCmd.IsConnected())
            {
                if (_tskMon == null || (_tskMon != null && _tskMon.IsCompleted))
                {
                    _tskMon = Task.Run(() =>
                    {
                        //「FWエラー取得」コマンドの要求を送信
                        return Program.PrpJigCmd.SendCmd_GetFwError(ref _lstFwErrMsg);
                    });
                    strErrMsg = await _tskMon;
                    if (strErrMsg == null)
                    {
                        strFwErrMsg = string.Empty;
                        foreach (string strMsg in _lstFwErrMsg)
                        {
                            strFwErrMsg += (strMsg + "\r\n");
                        }
                        if (textBox_FwErr.Text != strFwErrMsg)
                        {
                            textBox_FwErr.Text = strFwErrMsg;
                        }
                        this.Text = _strAppName + " - " + Program.PrpJigCmd.PrpConnectName + " - " + "Monitoring";
                    }
                    else
                    {
                        this.Text = _strAppName + " - " + "Monitor stopped";
                        AppendAppLogText(true, strErrMsg);
                    }
                }
            }
            else
            {
                this.Text = _strAppName + " - " + "Monitor stopped";
            }
        }

        // USBモードのラジオボタンをONにした時
        private void radioButton_UsbMode_CheckedChanged(object sender, EventArgs e)
        {
            if (true == radioButton_UsbMode.Checked)
            {
                comboBox_Port.Enabled = true;
                textBox_ServerIpAddr.Enabled = false;
            }
            else
            {
                comboBox_Port.Enabled = false;
                textBox_ServerIpAddr.Enabled = true;
            }
        }

        /// <summary>
        /// 「接続/切断」ボタンを押した時
        /// </summary>
        private void button_Connect_Click(object sender, EventArgs e)
        {
            if (radioButton_UsbMode.Checked)// USBモードの場合
            {
                Program.PrpJigCmd = Program.PrpJigSerial;
            }
            else // Wi-Fiモードの場合
            {
                Program.PrpJigCmd = Program.PrpJigTcpClient;
            }

            if (label_ConnectStatus.Text == STR_LBL_DISCONNECT) // 切断済みの場合
            {
                // 接続する
                Connect();
            }
            else // 接続済みの場合
            {
                // 切断する
                Disconnect();
            }
        }

        /// <summary>
        /// 接続する
        /// </summary>
        private void Connect()
        {
            string strMakerName = STR_NOT_DISPLAYED; // メーカー名
            string strFwName = STR_NOT_DISPLAYED;    // FW名
            string strFwVer = STR_NOT_DISPLAYED;     // FWバージョン
            string strBoardId = STR_NOT_DISPLAYED;   // ボードID
            string strParam; // COMポート名/IPアドレス
            string strErrMsg = null;

            if (radioButton_UsbMode.Checked) // USBモードの場合
            {
                if (comboBox_Port.Items.Count <= 0)
                {
                    strErrMsg = "There are no COM ports recognized by Windows.\r\nPlease connect the microcontroller board to the PC via USB and then restart this application.";
                    UI.ShowErrMsg(this, strErrMsg);
                    return;
                }
                strParam = comboBox_Port.Text.Trim(); // COMポート名
            }
            else
            {
                strParam = textBox_ServerIpAddr.Text.Trim(); // IPアドレス
            }

            AppendAppLogText(false, "Try connecting...");
          
            DateTime dt_start = DateTime.Now;
            DateTime dt_end;
            TimeSpan ts;
            do
            {
                // 接続する
                strErrMsg = Program.PrpJigCmd.Connect((Object)strParam);
                if (strErrMsg == null)
                {
                    break;
                }
                dt_end = DateTime.Now;
                ts = dt_end - dt_start;
            } while (ts.Seconds < RECONNECT_TIME);
                
            if (strErrMsg == null)
            {
                //「FW情報取得」コマンドの要求を送信
                strErrMsg = Program.PrpJigCmd.SendCmd_GetFwInfo(out strMakerName, out strFwName, out strFwVer, out strBoardId);
                if (strErrMsg != null)
                {
                    strErrMsg = "Firmware information could not be gotten from the microcontroller after connection.\n\n" + strErrMsg;
                }
            }
            
            if (strErrMsg == null) // コマンドが成功した場合
            {
                // [表示を更新]
                // ラジオボタンを無効に設定
                radioButton_UsbMode.Enabled = false;
                radioButton_Wifi.Enabled = false;
                // COMポート名一覧のコンボボックスを無効に設定
                comboBox_Port.Enabled = false;
                // IPアドレスのテキストボックスを無効に設定
                textBox_ServerIpAddr.Enabled = false;
                // 接続状態
                AppendAppLogText(false, "connected");
                label_ConnectStatus.Text = STR_LBL_CONNECT;
                label_ConnectStatus.BackColor = UI.MonGreen;
                // ボタンの表示を「切断する」に変更
                button_Connect.Text = STR_BTN_DISCONNECT;
                // FW名
                Str.PrpFwName = strFwName;
                label_FwName.Text = strFwName;
                // FWバージョン
                label_FwVer.Text = strFwVer;
                // ボードID
                label_BoardId.Text = strBoardId;
                // 子フォーム表示ボタンを有効に設定
                EnableFormButton(true);
            }
            else // 接続が失敗 または コマンドが失敗した場合
            {
                UI.ShowErrMsg(this, strErrMsg);
                // 切断する
                Program.PrpJigCmd.Disconnect();
            }
        }

        /// <summary>
        /// 切断する
        /// </summary>
        private void Disconnect()
        {
            string strFwName = STR_NOT_DISPLAYED;  // FW名
            string strFwVer = STR_NOT_DISPLAYED;   // FWバージョン
            string strBoardId = STR_NOT_DISPLAYED; // ボードID
            
            // 切断する
            Program.PrpJigCmd.Disconnect();
            // [表示を更新]
            // ラジオボタンを有効に設定
            radioButton_UsbMode.Enabled = true;
            radioButton_Wifi.Enabled = true;
            // COMポート名一覧のコンボボックスを有効に設定
            comboBox_Port.Enabled = true;
            // IPアドレスのテキストボックスを有効に設定
            textBox_ServerIpAddr.Enabled = true;
            // 接続状態
            AppendAppLogText(false, "disconnected");
            label_ConnectStatus.Text = STR_LBL_DISCONNECT;
            label_ConnectStatus.BackColor = UI.MonRed;
            // ボタンの表示を「接続する」に変更
            button_Connect.Text = STR_BTN_CONNECT;
            // FW名
            label_FwName.Text = strFwName;
            // FWバージョン
            label_FwVer.Text = strFwVer;
            // ボードID
            label_BoardId.Text = strBoardId;
            // 子フォーム表示ボタンを無効に設定
            EnableFormButton(false);     
        }

        /// <summary>
        /// 再接続する
        /// </summary>
        /// <remarks>
        /// マイコンが再起動されるようなコマンドが成功した後に本関数を使用する
        /// </remarks>
        public void Reconnect()
        {
            // 他のフォームから本関数が呼ばれた時に、メインフォームが既に破棄されている場合は何もしない
            if (true == this.IsDisposed)
            {
                return;
            }

            // 切断する
            Disconnect();
            AppendAppLogText(false, "Try reconnecting...");
            // マイコンが再起動するのを待つ
            Thread.Sleep(REBOOT_WAIT);
            // 接続する
            Connect();
        }

        /// <summary>
        /// 子フォーム表示ボタンの有効/無効を設定
        /// </summary>
        void EnableFormButton(bool bEnable)
        {
            foreach (Button btn in _lstButton)
            {
                btn.Enabled = bEnable;
            }
        }

        /// <summary>
        /// 「MTRX」ボタンを押した時
        /// </summary>
        private void button_Mtrx_Click(object sender, EventArgs e)
        {
            // MTRXフォームを表示
            ShowChildForm((Button)sender);
        }

        /// <summary>
        /// 子フォーム表示ボタンに応じた子フォームを表示
        /// </summary>
        private void ShowChildForm(Button button)
        {
            Form frm = null;

            if (button == button_Mtrx) // MTRX
            {
                frm = _formMtrx;
                if (frm == null || frm.IsDisposed)
                {
                    frm = _formMtrx = new FormMtrx();
                    frm.Show();
                }
            }
            else
            {
                // 無処理
            }

            // 一時的に子フォームを最前面に表示
            frm.TopMost = true;
            frm.TopMost = false;
            // 子フォームが最小化されている時、元の状態に戻す
            frm.WindowState = FormWindowState.Normal;
        }

        /// <summary>
        /// 「Appログ」テキストボックスにログを追加する
        /// </summary>
        public void AppendAppLogText(bool bError, string strMsg)
        {
            string strLog;

            // 他のフォームから本関数が呼ばれた時に、メインフォームが既に破棄されている場合は何もしない
            if (true == this.IsDisposed)
            {
                return;
            }

            // 送信コマンドの応答待ちをキャンセルした時のメッセージを表示しないようにする
            if (strMsg == JigCmd.STR_MSG_WAIT_RES_CANCEL)
            {
                // 無処理
            }
            else
            {
                if (bError)
                {
                    strMsg = "Err!!! " + strMsg;
                }
                strLog = "[" + DateTime.Now.ToString("HH:mm:ss") + "]" + strMsg + "\r\n";
                textBox_AppLog.AppendText(strLog);
            }
        }

        /// <summary>
        /// 「Appエラークリア」ボタンを押した時
        /// </summary>
        private void button_ClearAppLog_Click(object sender, EventArgs e)
        {
            textBox_AppLog.Text = string.Empty;
        }

        /// <summary>
        /// 「FWエラークリア」ボタンを押した時
        /// </summary>
        private async void button_ClearFwErr_Click(object sender, EventArgs e)
        {
            string strErrMsg;

            this.Enabled = false;
            strErrMsg = await Task.Run(() =>
            {
                //「FWエラークリア」コマンドの要求を送信
                return Program.PrpJigCmd.SendCmd_ClearFwError();
            });
            this.Enabled = true;

            if (strErrMsg == null)
            {
                // 無処理
            }
            else
            {
                UI.ShowErrMsg(this, strErrMsg);
            }
        }

        /// <summary>
        /// テキストボックスがキープレスされた時に半角のみ許可
        /// </summary>
        private void textBox_HalfWidth_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;

            // バックスペースは許可（制御文字のうち 0x08）
            if (c == '\b') return;

            // Ctrl + V (貼り付け) を許可
            if ((ModifierKeys & Keys.Control) == Keys.Control && c == 0x16) return;

            // 表示可能なASCII文字（0x20〜0x7E）を許可・・・英数字と記号
            if (c >= 0x20 && c <= 0x7E) return;

            // それ以外（全角や制御文字）は無効化
            e.Handled = true;
        }
    }
}
