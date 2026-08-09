// Copyright © 2024 Shiomachi Software. All rights reserved.


using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO.Ports;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using JigLib;

namespace JigApp
{
    public partial class FormMain : Form
    {
        /// <summary>
        /// 未接続状態などで表示する代替文字列
        /// </summary>
        private const string STR_NOT_DISPLAYED = "---";

        /// <summary>
        /// LEDマトリクスの行数
        /// </summary>
        private const int MTRX_ROWS = 32;
        /// <summary>
        /// パックして送信されるマトリクスデータの行数（元の行数を2で割った値）
        /// </summary>
        private const int MTRX_ROWS_SHOW = MTRX_ROWS / 2;
        /// <summary>
        /// LEDマトリクスの列数
        /// </summary>
        private const int MTRX_COLS = 64;
        /// <summary>
        /// 1枚分のマトリクスデータのサイズ
        /// </summary>
        private const int MTRX_DATA_SIZE = MTRX_COLS * MTRX_ROWS * 3;
        /// <summary>
        /// マトリクスデータ更新コマンドで1度に送信できるマトリクスデータの枚数
        /// </summary>
        private const int MTRX_SEND_MAX = 10;
        /// <summary>
        /// 接続ボタンの「接続する」状態の表示テキスト
        /// </summary>
        private const string STR_BTN_CONNECT = "connect";
        /// <summary>
        /// 接続ボタンの「切断する」状態の表示テキスト
        /// </summary>
        private const string STR_BTN_DISCONNECT = "disconnect";
        /// <summary>
        /// 接続ステータスラベルの「接続済み」状態の表示テキスト
        /// </summary>
        private const string STR_LBL_CONNECT = "connected";
        /// <summary>
        /// 接続ステータスラベルの「未接続」状態の表示テキスト
        /// </summary>
        private const string STR_LBL_DISCONNECT = "disconnected";

        /// <summary>
        /// 再接続のリトライを行う最大時間(秒)
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
        /// <summary>
        /// コネクション接続状況に応じて有効・無効を切り替えるボタンのリスト
        /// </summary>
        private List<Button> _lstButton = new List<Button>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FormMain()
        {
            InitializeComponent();
            // 自分のインスタンスを保存
            Inst = this;
            // COMポートのオープン状況に連動するボタンを登録
            _lstButton.Add(button_OpenMtrxFile);
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
            this.Text = _strAppName;
            // アプリのバージョンを表示
            FileVersionInfo verInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            label_AppVer.Text = verInfo.FileVersion;
            // COMポート名の一覧をコンボボックスに追加
            AddSerialPortToList();
            // 切り抜きモードの一覧をコンボボックスに追加
            comboBox_CropMode.Items.Add("No crop (Default)");
            comboBox_CropMode.Items.Add("Crop center area (1/2 width & height)");
            comboBox_CropMode.Items.Add("Crop center area (1/4 width & height)");
            comboBox_CropMode.SelectedIndex = 0;
            // 接続状態ラベルの色を設定
            label_ConnectStatus.BackColor = UI.MonRed;
            // 接続状態に依存するボタンを無効に設定
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

        private void button_Connect_Click(object sender, EventArgs e)
        {
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
            string strParam; // COMポート名
            string strErrMsg = null;

            if (comboBox_Port.Items.Count <= 0)
            {
                strErrMsg = "There are no COM ports recognized by Windows.\r\nPlease connect the microcontroller board to the PC via USB and then restart this application.";
                UI.ShowErrMsg(this, strErrMsg);
                return;
            }
            strParam = comboBox_Port.Text.Trim(); // COMポート名

            AppendAppLogText(false, "Try connecting...");

            DateTime dtStart = DateTime.Now;
            TimeSpan ts;
            do
            {
                // 接続する
                strErrMsg = Program.PrpJigCmd.Connect(strParam);
                if (strErrMsg == null)
                {
                    break;
                }
                Thread.Sleep(100); // 接続失敗時の再試行まで少し待機（CPU高負荷およびUI完全フリーズの防止）
                ts = DateTime.Now - dtStart;
            } while (ts.TotalSeconds < RECONNECT_TIME);

            if (strErrMsg == null)
            {
                // 「FW情報取得」コマンドの要求を送信
                strErrMsg = Program.PrpJigCmd.SendCmd_GetFwInfo(out _, out string strFwName, out string strFwVer, out string strBoardId);
                if (strErrMsg == null) // コマンドが成功した場合
                {
                    // [表示を更新]
                    // COMポート名一覧のコンボボックスを無効に設定
                    comboBox_Port.Enabled = false;
                    // 接続状態
                    AppendAppLogText(false, "connected");
                    label_ConnectStatus.Text = STR_LBL_CONNECT;
                    label_ConnectStatus.BackColor = UI.MonGreen;
                    // ボタンの表示を「切断する」に変更
                    button_Connect.Text = STR_BTN_DISCONNECT;
                    // FW名
                    label_FwName.Text = strFwName;
                    // FWバージョン
                    label_FwVer.Text = strFwVer;
                    // ボードID
                    label_BoardId.Text = strBoardId;
                    // コネクション依存のボタンを有効に設定
                    EnableFormButton(true);
                }
                else // コマンドが失敗した場合
                {
                    strErrMsg = "Firmware information could not be gotten from the microcontroller after connection.\n\n" + strErrMsg;
                    UI.ShowErrMsg(this, strErrMsg);
                    // 切断する
                    Program.PrpJigCmd.Disconnect();
                }
            }
            else // 接続が失敗した場合
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
            // 切断する
            Program.PrpJigCmd.Disconnect();
            // [表示を更新]
            // COMポート名一覧のコンボボックスを有効に設定
            comboBox_Port.Enabled = true;
            // 接続状態
            AppendAppLogText(false, "disconnected");
            label_ConnectStatus.Text = STR_LBL_DISCONNECT;
            label_ConnectStatus.BackColor = UI.MonRed;
            // ボタンの表示を「接続する」に変更
            button_Connect.Text = STR_BTN_CONNECT;
            // FW名
            label_FwName.Text = STR_NOT_DISPLAYED;
            // FWバージョン
            label_FwVer.Text = STR_NOT_DISPLAYED;
            // ボードID
            label_BoardId.Text = STR_NOT_DISPLAYED;
            // コネクション依存のボタンを無効に設定
            EnableFormButton(false);
        }

        /// <summary>
        /// 接続状態に依存するボタンの有効/無効を設定
        /// </summary>
        private void EnableFormButton(bool bEnable)
        {
            foreach (Button btn in _lstButton)
            {
                btn.Enabled = bEnable;
            }
        }

        /// <summary>
        /// 「Convert mp4 to mtrx file」ボタンを押した時
        /// </summary>
        private async void button_ConvertMp4ToMtrxFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "mp4 file(*.mp4)|*.mp4";   // フィルタを設定
                dlg.FilterIndex = 1;                    // フィルタの初期選択インデックス
                dlg.Title = "select mp4 file";          // ダイアログのタイトル
                dlg.RestoreDirectory = true;            // 終了時にカレントディレクトリを復元
                // mp4のファイル選択ダイアログを表示
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Enabled = false; // フォーム全体を無効にして誤操作防止

                    string strMp4Path = dlg.FileName;
                    string strMtrxName = Path.GetFileNameWithoutExtension(strMp4Path) + ".mtrx";
                    label_FileName.Text = Path.GetFileName(strMp4Path) + " => " + strMtrxName;
                    label_ConvertProgress.Text = "0%";

                    // [mtrxファイルのフルパスを作成]
                    // mtrxのフルパスを作成
                    string strMtrxPath = Path.Combine(Path.GetDirectoryName(strMp4Path), strMtrxName);

                    int cropMode = comboBox_CropMode.SelectedIndex;

                    string strErrMsg = await Task.Run(() =>
                    {
                        // mp4ファイルをmtrxファイルに変換する
                        return ConvertMp4ToMtrxFile(strMp4Path, strMtrxPath, cropMode);
                    });

                    if (strErrMsg != null)
                    {
                        UI.ShowErrMsg(this, strErrMsg);
                    }

                    this.Enabled = true; // フォームを有効
                }
            }
        }

        /// <summary>
        /// mp4ファイルをmtrxファイルに変換する
        /// </summary>
        private string ConvertMp4ToMtrxFile(string strMp4Path, string strMtrxPath, int cropMode)
        {
            string strErrMsg = null;

            // VideoCaptureオブジェクトを作成し、指定されたmp4ファイルを開く
            using (var capture = new VideoCapture(strMp4Path))
            {
                // mp4ファイルを開けたかチェック
                if (!capture.IsOpened())
                {
                    strErrMsg = "Could not open mp4 file";
                    return strErrMsg;
                }

                // 動画フレーム数を取得
                int frameCount = (int)capture.Get(CaptureProperty.FrameCount);
                int iFrame = 0; // 読み込み済みのフレーム数

                try
                {
                    // ファイルをあらかじめ開いておく(毎フレームのオープン/クローズによるファイルロック競合およびオーバーヘッドを防止)
                    using (BinaryWriter writer = new BinaryWriter(File.Open(strMtrxPath, FileMode.Create, FileAccess.Write)))
                    {
                        while (true)
                        {
                            // フレームを格納するためのMatオブジェクトを初期化
                            Mat img = null;
                            try
                            {
                                img = new Mat();
                                // フレームを読み込む
                                if (!capture.Read(img))
                                {
                                    img.Dispose();
                                    img = null;
                                    break; // 読み込めなくなったら（＝本当の終端）ループを抜ける
                                }

                                // チャンネル数のチェック・BGRへの強制変換(アルファチャンネル付きやモノクロ動画によるクラッシュ防止)
                                if (img.Channels() == 4)
                                {
                                    var bgrImg = new Mat();
                                    Cv2.CvtColor(img, bgrImg, ColorConversion.BgraToBgr);
                                    img.Dispose();
                                    img = bgrImg;
                                }
                                else if (img.Channels() == 1)
                                {
                                    var bgrImg = new Mat();
                                    Cv2.CvtColor(img, bgrImg, ColorConversion.GrayToBgr);
                                    img.Dispose();
                                    img = bgrImg;
                                }

                                // [フレームが縦長画像の場合(Height > Width), 画像を90度左に回転して横長にする]
                                if (img.Height > img.Width)
                                {
                                    // 画像を左に90度回転
                                    var rotated = RotateLeft(img);
                                    img.Dispose();
                                    img = rotated;
                                }

                                // [画像の中央部分を切り抜く(目的の縦横比 幅:高さ = 2:1)]
                                const double ASPECT_RATIO = 2.0;

                                // 元の画像に収まる最大のターゲットサイズを計算
                                int targetWidth, targetHeight;
                                int xStart, yStart;

                                // 元の画像の幅を基準とした場合に必要な高さ
                                int requiredHeight = (int)(img.Width / ASPECT_RATIO);

                                if (requiredHeight <= img.Height)
                                {
                                    // 幅基準で計算した高さが、元の画像高さに収まる場合(元の画像が横長か正方形に近い)
                                    // => 幅いっぱいにトリミングし、高さを中央寄せする
                                    targetWidth = img.Width;
                                    targetHeight = requiredHeight;

                                    // Y座標を中央寄せで計算
                                    yStart = (img.Height - targetHeight) / 2;
                                    xStart = 0; // 幅は元の画像と同じなのでX座標は0
                                }
                                else // requiredHeight > img.Height
                                {
                                    // 高さ基準で計算した幅が、元の画像幅に収まる場合(元の画像が縦長)
                                    // => 高さいっぱいにトリミングし、幅を中央寄せする
                                    targetHeight = img.Height;
                                    targetWidth = (int)(img.Height * ASPECT_RATIO);

                                    // X座標を中央寄せで計算
                                    xStart = (img.Width - targetWidth) / 2;
                                    yStart = 0; // 高さは元の画像と同じなのでY座標は0
                                }

                                // ユーザーが選択した切り抜きモードに応じて切り抜き範囲をさらに縮小(中央中心)
                                double cropScale = 1.0;
                                if (cropMode == 1) // 1/2を切り抜く
                                {
                                    cropScale = 0.5;
                                }
                                else if (cropMode == 2) // 1/4を切り抜く
                                {
                                    cropScale = 0.25;
                                }

                                int finalWidth = (int)(targetWidth * cropScale);
                                int finalHeight = (int)(targetHeight * cropScale);
                                int finalX = xStart + (targetWidth - finalWidth) / 2;
                                int finalY = yStart + (targetHeight - finalHeight) / 2;

                                 // 入力画像から指定した範囲を切り抜く(メモリ破損防止のためディープコピーを生成してから元のメモリをDispose)
                                 Mat cropped;
                                 using (var subMat = new Mat(img, new OpenCvSharp.CPlusPlus.Rect(finalX, finalY, finalWidth, finalHeight)))
                                 {
                                     cropped = subMat.Clone();
                                 }
                                 img.Dispose();
                                 img = cropped;

                                // 画像を64×32ピクセルにリサイズ(デフォルトのバイリニア補間を適用)
                                var resized = new Mat();
                                Cv2.Resize(img, resized, new OpenCvSharp.CPlusPlus.Size(MTRX_COLS, MTRX_ROWS), 0, 0, Interpolation.Linear);
                                img.Dispose();
                                img = resized;

                                // [マトリクスデータファイル(.mtrx)の書き込み]
                                for (int y = 0; y < MTRX_ROWS; y++)
                                {
                                    for (int x = 0; x < MTRX_COLS; x++)
                                    {
                                        Vec3b pixel = img.At<Vec3b>(y, x);
                                        writer.Write(pixel.Item2); // R
                                        writer.Write(pixel.Item1); // G
                                        writer.Write(pixel.Item0); // B
                                    }
                                }

                                img.Dispose();
                                img = null;

                                iFrame++;
                                // 進捗のUIを更新
                                UpdateConvertProgress(frameCount, iFrame);
                            }
                            finally
                            {
                                if (img != null)
                                {
                                    img.Dispose();
                                }
                            }
                        }
                    }
                    // 正常終了した場合のみ100%にする
                    UpdateConvertProgress(frameCount, frameCount);
                }
                catch (Exception ex)
                {
                    strErrMsg = ex.Message;
                }
            }

            return strErrMsg;
        }

        /// <summary>
        /// 画像を左に90度回転
        /// </summary>
        private Mat RotateLeft(Mat img)
        {
            // 画像を左に90度回転
            var transposed = new Mat();
            Cv2.Transpose(img, transposed);  // 転置(行列を入れ替え)

            var flipped = new Mat();
            Cv2.Flip(transposed, flipped, FlipMode.Y); // Y軸で反転

            transposed.Dispose();
            return flipped;
        }



        /// <summary>
        /// 進捗のUIを更新
        /// </summary>
        private void UpdateConvertProgress(int frameCount, int frameNo)
        {
            // ゼロ除算防止および負の値防止
            int safeFrameCount = Math.Max(1, frameCount);

            progressBar_Convert.Invoke((Action)(() =>
            {
                if (frameNo == 1)
                {
                    progressBar_Convert.Minimum = 0;
                    progressBar_Convert.Maximum = safeFrameCount;
                    progressBar_Convert.Value = 0;
                }
                else
                {
                    // 実際のフレーム数が推測値を超えた場合のクラッシュ防止(ArgumentOutOfRangeException)
                    progressBar_Convert.Value = Math.Min(frameNo, progressBar_Convert.Maximum);
                }
            }));

            label_ConvertProgress.Invoke((Action)(() =>
            {
                // 100% を超えないように制限およびゼロ除算防止
                int percent = Math.Min(100, 100 * frameNo / safeFrameCount);
                label_ConvertProgress.Text = percent.ToString() + "%";
            }));
        }

        /// <summary>
        /// 「Open mtrx file」ボタンを押した時
        /// </summary>
        private async void button_OpenMtrxFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "matrix data file(*.mtrx)|*.mtrx"; // フィルタを設定
                dlg.FilterIndex = 1;                            // フィルタの初期選択インデックス
                dlg.Title = "select matrix data file";          // ダイアログのタイトル
                dlg.RestoreDirectory = true;                    // 終了時にカレントディレクトリを復元
                // ファイル選択ダイアログを表示
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Enabled = false; // フォーム全体を無効にして誤操作防止

                    string strFileName = dlg.FileName;

                    string strErrMsg = await Task.Run(() =>
                    {
                        // 「マトリクスデータクリア」コマンドを送信
                        string strErr = Program.PrpJigCmd.SendCmd_ClearMatrix();
                        if (strErr != null)
                        {
                            return strErr;
                        }

                        // mtrxファイルの読み込み
                        byte[] allMatrixData;
                        try
                        {
                            allMatrixData = File.ReadAllBytes(strFileName);
                        }
                        catch (Exception ex)
                        {
                            return ex.Message;
                        }

                        int delayedBatchCount = 0;
                        int remainNum = allMatrixData.Length / MTRX_DATA_SIZE; // 残り枚数
                        int sentNum = 0; // 送信済み枚数
                        while (remainNum > 0) // 残り枚数が0より大きい場合
                        {
                            // 送信枚数を計算
                            int sendNum = (remainNum > MTRX_SEND_MAX) ? MTRX_SEND_MAX : remainNum;

                            // 送信枚数分のマトリクスデータをバッチ単位で1度だけ作成（リトライ時の再確保を防ぎ、GC負荷を極小化）
                            byte[] sendMatrixData = new byte[MTRX_DATA_SIZE * sendNum]; 
                            Array.Copy(
                                allMatrixData,            // 全マトリクスデータ(コピー元)
                                sentNum * MTRX_DATA_SIZE, // コピー開始位置
                                sendMatrixData,           // 送信枚数分のマトリクスデータ(コピー先)
                                0,                        // コピー先での開始位置(通常は0)
                                sendNum * MTRX_DATA_SIZE  // コピーする要素数
                            );

                            // 送信完了するまで内側でリトライループ
                            System.Diagnostics.Stopwatch swBatch = new System.Diagnostics.Stopwatch();
                            int retryCount = 0;
                            swBatch.Start();

                             while (true)
                             {
                                 // まずバッファの空き状況を確認 (0バイト要求のため極めて軽量)
                                 string strBufErr = Program.PrpJigCmd.SendCmd_GetBufStatus();
                                 if (strBufErr == "A failure response was received from the microcontroller. (There is no space in the buffer)")
                                 {
                                     retryCount++;
                                     // 1フレーム再生時間(33.3ms)に近い30ms待機し、USB通信の無駄な連打とマイコンCPUの負荷を防止
                                     Thread.Sleep(30);
                                     continue; // バッファが空くまで待つ
                                 }
                                 else if (strBufErr != null)
                                 {
                                     return strBufErr; // その他の通信エラー
                                 }

                                 System.Diagnostics.Stopwatch swSend = System.Diagnostics.Stopwatch.StartNew();
                                 // バッファに空きがあるので、「マトリクスデータ更新」コマンドを送信 (確実に成功する)
                                 strErr = Program.PrpJigCmd.SendCmd_UpdateMatrix(sendMatrixData);
                                 swSend.Stop();

                                if (strErr == null)
                                {
                                    swBatch.Stop();
                                    System.Diagnostics.Debug.WriteLine($"[Profile] Batch {sentNum/MTRX_SEND_MAX}: Sent {sendNum} frames. TotalTime={swBatch.ElapsedMilliseconds}ms (LastSend={swSend.ElapsedMilliseconds}ms), Retries={retryCount}");
                                    
                                    // 1フレームあたり33.3msで消費される。1バッチ(sendNum)の消費時間を超えているか判定。
                                    double limitTime = sendNum * (1000.0 / 30.0);
                                    double effectiveTime = swBatch.ElapsedMilliseconds - (retryCount * 30);
                                    if (effectiveTime > limitTime)
                                    {
                                        delayedBatchCount++;
                                    }

                                    remainNum -= sendNum; // 残り枚数を更新
                                    sentNum += sendNum;   // 送信済み枚数を更新
                                    break; // 送信成功、次のバッチへ
                                }
                                else if (strErr == "A failure response was received from the microcontroller. (There is no space in the buffer)")
                                {
                                    retryCount++;
                                    // 1フレーム再生時間(33.3ms)に近い30ms待機し、USB通信の無駄な連打とマイコンCPUの負荷を防止
                                    Thread.Sleep(30);
                                }
                                else
                                {
                                    return strErr;
                                }
                            }
                        }

                        // 判定結果を英語でデバッグ出力およびアプリログに表示
                        if (delayedBatchCount > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"[Profile] Result: Processing DELAYED. {delayedBatchCount} batches experienced delays.");
                            FormMain.Inst.Invoke((Action)(() => FormMain.Inst.AppendAppLogText(true, $"Processing warning: {delayedBatchCount} batches experienced transmission delays.")));
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("[Profile] Result: Processing SUCCESS. All data was sent in time.");
                            FormMain.Inst.Invoke((Action)(() => FormMain.Inst.AppendAppLogText(false, "Processing success: All data was sent in time.")));
                        }

                        return null;
                    });

                    if (strErrMsg != null)
                    {
                        UI.ShowErrMsg(this, strErrMsg);
                    }

                    this.Enabled = true; // フォームを有効
                }
            }
        }

        /// <summary>
        /// 「Appログ」テキストボックスにログを追加する
        /// </summary>
        public void AppendAppLogText(bool bError, string strMsg)
        {
            string strLog;

            // 他のフォームから本関数が呼ばれた時に、メインフォームが既に破棄されている場合は何もしない
            if (this.IsDisposed)
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
                string strFormattedMsg = strMsg;
                if (bError)
                {
                    strFormattedMsg = "Err!!! " + strMsg;
                }
                strLog = "[" + DateTime.Now.ToString("HH:mm:ss") + "]" + strFormattedMsg + "\r\n";
                textBox_AppLog.AppendText(strLog);
            }
        }

        /// <summary>
        /// 「Appログクリア」ボタンを押した時
        /// </summary>
        private void button_ClearAppLog_Click(object sender, EventArgs e)
        {
            textBox_AppLog.Text = string.Empty;
        }
    }
}
