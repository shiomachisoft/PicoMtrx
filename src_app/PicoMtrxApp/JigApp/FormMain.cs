// Copyright © 2024 Shiomachi Software. All rights reserved.

//#define USE_DITHERING // ディザリングを使用する

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
        private const int MTRX_DATA_SIZE = MTRX_COLS * MTRX_ROWS_SHOW;
        /// <summary>
        /// マトリクスデータ更新コマンドで1度に送信できるマトリクスデータの枚数
        /// </summary>
        private const int MTRX_SEND_MAX = 30;
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

                    string strErrMsg = await Task.Run(() =>
                    {
                        // mp4ファイルをmtrxファイルに変換する
                        return ConvertMp4ToMtrxFile(strMp4Path, strMtrxPath);
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
        private string ConvertMp4ToMtrxFile(string strMp4Path, string strMtrxPath)
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

                                // 入力画像から指定した範囲を切り抜く(メモリ破損防止のためディープコピーを生成してから元のメモリをDispose)
                                var cropped = new Mat(img, new OpenCvSharp.CPlusPlus.Rect(xStart, yStart, targetWidth, targetHeight)).Clone();
                                img.Dispose();
                                img = cropped;

                                // 画像を64×32ピクセルにリサイズ(デフォルトのバイリニア補間を適用)
                                var resized = new Mat();
                                Cv2.Resize(img, resized, new OpenCvSharp.CPlusPlus.Size(MTRX_COLS, MTRX_ROWS), 0, 0, Interpolation.Linear);
                                img.Dispose();
                                img = resized;

#if USE_DITHERING
                                // 鮮鋭化フィルタを適用
                                img = ApplyUnsharpMasking(img, 1.0, 1.5);
                                // Floyd-Steinberg誤差拡散ディザリングによる減色
                                img = ConvertToDithered8Color(img);
#else
                                // 入力画像をRGB各2諧調の画像に変換(大津の2値化による減色)
                                img = ConvertToBilevel(img);
#endif
                                // [RGB各2諧調の画像からマトリクスデータを作成]
                                byte[,] matrixData = null; // 1枚分のマトリクスデータ
                                matrixData = MakeMatrixData(img);

                                img.Dispose();
                                img = null;

                                // [マトリクスデータファイル(.mtrx)の書き込み]
                                for (int y = 0; y < MTRX_ROWS_SHOW; y++)
                                {
                                    for (int x = 0; x < MTRX_COLS; x++)
                                    {
                                        writer.Write(matrixData[y, x]);
                                    }
                                }

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
        /// 入力画像をRGB各2諧調の画像に変換(減色)
        /// </summary>
        /// <remarks>
        /// 鮮鋭化フィルタ⇒大津の2値化を適用
        /// </remarks>
        private Mat ConvertToBilevel(Mat img)
        {
            var channels = Cv2.Split(img); // RGB成分

            var bilevelChannels = new Mat[3]; // RGB成分の2値画像

            // [各RGB成分に対して、鮮鋭化フィルタ⇒大津の2値化を適用]
            for (int i = 0; i < channels.Length; i++)
            {
                // 鮮鋭化フィルタを適用
                channels[i] = ApplyUnsharpMasking(channels[i], 3, 3);

                // [大津の2値化]
                bilevelChannels[i] = new Mat(); // RGB成分の2値画像
                double thresholdValue = 0; // Otsu法が自動計算するため0で良い
                double maxVal = 255;
                double otsuThreshold = Cv2.Threshold(
                    channels[i],
                    bilevelChannels[i],
                    thresholdValue,
                    maxVal,
                    // Binary と Otsu フラグを組み合わせて、大津の2値化を要求
                    type: ThresholdType.Binary | ThresholdType.Otsu
                );

                channels[i].Dispose(); // RGB成分を解放
            }

            // 各RGB成分の2値画像を結合
            var outImg = new Mat();
            Cv2.Merge(bilevelChannels, outImg);

            // RGB成分の2値画像を解放
            for (int i = 0; i < channels.Length; i++)
            {
                bilevelChannels[i].Dispose();
            }

            img.Dispose();

            return outImg;
        }

        /// <summary>
        /// 入力画像をRGB各2諧調の画像に変換(Floyd-Steinberg誤差拡散ディザリングによる減色)
        /// </summary>
        private Mat ConvertToDithered8Color(Mat img)
        {
            int width = img.Cols;
            int height = img.Rows;

            // 誤差を周囲に高精度に拡散するため、浮動小数点の配列に画像データを展開
            float[,,] bgrData = new float[height, width, 3];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vec3b pixel = img.At<Vec3b>(y, x);
                    bgrData[y, x, 0] = pixel.Item0; // B
                    bgrData[y, x, 1] = pixel.Item1; // G
                    bgrData[y, x, 2] = pixel.Item2; // R
                }
            }

            // Floyd-Steinberg 誤差拡散ディザリングの適用
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float oldB = bgrData[y, x, 0];
                    float oldG = bgrData[y, x, 1];
                    float oldR = bgrData[y, x, 2];

                    // 各チャンネルで127.5を閾値として0または255に丸める
                    // (これにより自動的に定義された8色のうち、RGB空間上で最短距離の色にマッピングされます)
                    float newB = (oldB > 127.5f) ? 255.0f : 0.0f;
                    float newG = (oldG > 127.5f) ? 255.0f : 0.0f;
                    float newR = (oldR > 127.5f) ? 255.0f : 0.0f;

                    // 誤差を算出
                    float errB = oldB - newB;
                    float errG = oldG - newG;
                    float errR = oldR - newR;

                    bgrData[y, x, 0] = newB;
                    bgrData[y, x, 1] = newG;
                    bgrData[y, x, 2] = newR;

                    // Floyd-Steinberg の配分比率に従って誤差を拡散
                    // 右: 7/16, 左下: 3/16, 下: 5/16, 右下: 1/16
                    if (x + 1 < width)
                    {
                        bgrData[y, x + 1, 0] += errB * 7.0f / 16.0f;
                        bgrData[y, x + 1, 1] += errG * 7.0f / 16.0f;
                        bgrData[y, x + 1, 2] += errR * 7.0f / 16.0f;
                    }
                    if (y + 1 < height)
                    {
                        if (x - 1 >= 0)
                        {
                            bgrData[y + 1, x - 1, 0] += errB * 3.0f / 16.0f;
                            bgrData[y + 1, x - 1, 1] += errG * 3.0f / 16.0f;
                            bgrData[y + 1, x - 1, 2] += errR * 3.0f / 16.0f;
                        }
                        bgrData[y + 1, x, 0] += errB * 5.0f / 16.0f;
                        bgrData[y + 1, x, 1] += errG * 5.0f / 16.0f;
                        bgrData[y + 1, x, 2] += errR * 5.0f / 16.0f;
                        if (x + 1 < width)
                        {
                            bgrData[y + 1, x + 1, 0] += errB * 1.0f / 16.0f;
                            bgrData[y + 1, x + 1, 1] += errG * 1.0f / 16.0f;
                            bgrData[y + 1, x + 1, 2] += errR * 1.0f / 16.0f;
                        }
                    }
                }
            }

            // 新たな Mat オブジェクトにデータを書き戻す
            var outImg = new Mat(height, width, MatType.CV_8UC3);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte b = (byte)Math.Max(0, Math.Min(255, (int)bgrData[y, x, 0]));
                    byte g = (byte)Math.Max(0, Math.Min(255, (int)bgrData[y, x, 1]));
                    byte r = (byte)Math.Max(0, Math.Min(255, (int)bgrData[y, x, 2]));
                    outImg.Set<Vec3b>(y, x, new Vec3b(b, g, r));
                }
            }

            img.Dispose();
            return outImg;
        }

        /// <summary>
        /// 鮮鋭化フィルタ
        /// </summary>
        private Mat ApplyUnsharpMasking(Mat img, double sigma, double amount)
        {
            Mat outImg = new Mat();

            using (var blurred = new Mat())
            {
                // 元画像をぼかす
                Cv2.GaussianBlur(img, blurred, new OpenCvSharp.CPlusPlus.Size(0, 0), sigma);

                // 鮮鋭化の計算
                Cv2.AddWeighted(img, 1.0 + amount, blurred, -amount, 0, outImg);
            }

            img.Dispose();

            return outImg;
        }

        /// <summary>
        /// RGB各2諧調の画像からマトリクスデータを作成
        /// </summary>
        private byte[,] MakeMatrixData(Mat img)
        {
            byte[,] matrixData = new byte[MTRX_ROWS_SHOW, MTRX_COLS]; // マトリクスデータの配列(出力配列)

            for (int y = 0; y < MTRX_ROWS; y++) // LEDマトリクスの行数だけ繰り返す
            {
                for (int x = 0; x < MTRX_COLS; x++) // LEDマトリクスの列数だけ繰り返す
                {
                    // 元画像のピクセル色を取得
                    Vec3b pixel = img.At<Vec3b>(y, x);
                    byte r = pixel.Item2;
                    byte g = pixel.Item1;
                    byte b = pixel.Item0;
                    byte color;
                    if (r == 0 && g == 0 && b == 255)
                    {
                        color = 0x01;       // Blue
                    }
                    else if (r == 0 && g == 255 && b == 0)
                    {
                        color = 0x02;       // Green
                    }
                    else if (r == 0 && g == 255 && b == 255)
                    {
                        color = 0x03;       // Cyan
                    }
                    else if (r == 255 && g == 0 && b == 0)
                    {
                        color = 0x04;       // Red
                    }
                    else if (r == 255 && g == 0 && b == 255)
                    {
                        color = 0x05;       // Purple
                    }
                    else if (r == 255 && g == 255 && b == 0)
                    {
                        color = 0x06;       // Yellow
                    }
                    else if (r == 255 && g == 255 && b == 255)
                    {
                        color = 0x07;       // White
                    }
                    else
                    {
                        color = 0;          // Black
                    }

                    if (y >= MTRX_ROWS_SHOW)
                    {
                        int yy = y - MTRX_ROWS_SHOW;
                        matrixData[yy, x] = (byte)((matrixData[yy, x] & 0x0F) | (color << 4));
                    }
                    else
                    {
                        matrixData[y, x] = (byte)((matrixData[y, x] & 0xF0) | color);
                    }
                }
            }

            return matrixData;
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

                        int remainNum = allMatrixData.Length / MTRX_DATA_SIZE; // 残り枚数
                        int sentNum = 0; // 送信済み枚数
                        while (remainNum > 0) // 残り枚数が0より大きい場合
                        {
                            // 送信枚数を計算
                            int sendNum = (remainNum > MTRX_SEND_MAX) ? MTRX_SEND_MAX : remainNum;

                            byte[] sendMatrixData = new byte[MTRX_DATA_SIZE * sendNum]; // 送信枚数分のマトリクスデータ

                            // sendMatrixDataにデータを格納
                            Array.Copy(
                                allMatrixData,            // 全マトリクスデータ(コピー元)
                                sentNum * MTRX_DATA_SIZE, // コピー開始位置
                                sendMatrixData,           // 送信枚数分のマトリクスデータ(コピー先)
                                0,                        // コピー先での開始位置(通常は0)
                                sendNum * MTRX_DATA_SIZE  // コピーする要素数
                            );

                            // 「マトリクスデータ更新」コマンドを送信
                            strErr = Program.PrpJigCmd.SendCmd_UpdateMatrix(sendMatrixData);
                            if (strErr == null)
                            {
                                remainNum -= sendNum; // 残り枚数を更新
                                sentNum += sendNum;   // 送信済み枚数を更新
                            }
                            else if (strErr == "A failure response was received from the microcontroller. (There is no space in the buffer)")
                            {
                                // バッファが空くまで少し待機してリトライ(ビジーウェイトによるCPU100%占有と通信連打を防止)
                                Thread.Sleep(100);
                            }
                            else
                            {
                                return strErr;
                            }
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
