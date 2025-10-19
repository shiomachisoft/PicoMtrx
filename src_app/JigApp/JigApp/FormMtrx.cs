using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rect = OpenCvSharp.CPlusPlus.Rect;

namespace JigApp
{
    public partial class FormMtrx : Form
    {
        /// <summary>
        /// LEDマトリクスの行数
        /// </summary>
        private const int MTRX_ROWS = 32;
        /// <summary>
        /// LEDマトリクスの行数を2で割った値
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
        /// コンストラクタ
        /// </summary>
        public FormMtrx()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 「Convert mp4 to mtrx file」ボタンを押した時
        /// </summary>
        private async void button_ConvertMp4ToMtrxFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                string strErrMsg = null;

                dlg.Filter = "mp4 file(*.mp4)|*.mp4";   // フィルタを設定
                dlg.FilterIndex = 1;                    // フィルタの初期選択インデックス
                dlg.Title = "select mp4 file";          // ダイアログのタイトル
                dlg.RestoreDirectory = true;            // 終了時にカレントディレクトリを復元
                // mp4のファイル選択ダイアログを表示
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Enabled = false; // ボタンを無効

                    string strMp4Path = dlg.FileName;
                    string strMp4Name = Path.GetFileName(strMp4Path);
                    string strMtrxName = Path.GetFileNameWithoutExtension(strMp4Path) + ".mtrx";
                    label_FileName.Text = strMp4Name + " => " + strMtrxName;
                    label_ConvertProgress.Text = "0%";

                    // [mtrxファイルのフルパスを作成]
                    // mp4ファイルのディレクトリパスを取得
                    string strFolderPath = Path.GetDirectoryName(strMp4Path);
                    // mtrxのフルパスを作成
                    string strMtrxPath = strFolderPath + "\\" + strMtrxName;

                    strErrMsg = await Task.Run(() =>
                    {
                        // mp4ファイルをmtrxファイルに変換する
                        strErrMsg = ConvertMp4ToMtrxFile(strMp4Path, strMtrxPath);
                        return strErrMsg;
                    });

                    if (strErrMsg != null)
                    {
                        UI.ShowErrMsg(this, strErrMsg);
                    }

                    this.Enabled = true; // ボタンを有効
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
                for (int iFrame = 0; iFrame < frameCount; iFrame++) // フレームの数だけ繰り返す
                {
                    // 指定したフレーム番号にキャプチャ位置を移動(シーク)
                    capture.Set(CaptureProperty.PosFrames, iFrame);
                    // フレームを格納するためのMatオブジェクトを初期化
                    var img = new Mat();
                    // フレームを読み込む
                    if (true == capture.Read(img))
                    {
                        // [フレームが縦長画像の場合(Height > Width)、画像を90度左に回転して横長にする]
                        
                        if (img.Height > img.Width)
                        {
                            // 画像を左に90度回転
                            RotateLeft(img);
                        }

                        // [画像の中央分を切り抜く(目的の縦横比(幅:高さ = 2:1)]

                        const double ASPECT_RATIO = 2.0;

                        // 元の画像に収まる最大のターゲットサイズを計算
                        int targetWidth, targetHeight;
                        int x_start, y_start;

                        // 元の画像の幅を基準とした場合に必要な高さ
                        int requiredHeight = (int)(img.Width / ASPECT_RATIO);

                        if (requiredHeight <= img.Height)
                        {
                            // 幅基準で計算した高さが、元の画像高さに収まる場合(元の画像が横長か正方形に近い)
                            // => 幅いっぱいにトリミングし、高さを中央寄せする
                            targetWidth = img.Width;
                            targetHeight = requiredHeight;

                            // Y座標を中央寄せで計算
                            y_start = (img.Height - targetHeight) / 2;
                            x_start = 0; // 幅は元の画像と同じなのでX座標は0
                        }
                        else // requiredHeight > img.Height
                        {
                            // 高さ基準で計算した幅が、元の画像幅に収まる場合(元の画像が縦長)
                            // => 高さいっぱいにトリミングし、幅を中央寄せする
                            targetHeight = img.Height;
                            targetWidth = (int)(img.Height * ASPECT_RATIO);

                            // X座標を中央寄せで計算
                            x_start = (img.Width - targetWidth) / 2;
                            y_start = 0; // 高さは元の画像と同じなのでY座標は0
                        }

                        // 入力画像から指定した範囲を切り抜く
                        img = CropImage(img, x_start, y_start, targetWidth, targetHeight);

                        // 画像を64×32ピクセルにリサイズ
                        Cv2.Resize(img, img, new OpenCvSharp.CPlusPlus.Size(MTRX_COLS, MTRX_ROWS));

                        // 入力画像をRGB各2諧調の画像に変換(減色)
                        img = ConvertToBilevel(img);

                        // [RGB各2諧調の画像からマトリクスデータを作成]
                        byte[,] matrixData = null; // 1枚分のマトリクスデータ
                        matrixData = MakeMatrixData(img);

                        img.Dispose();

                        // [マトリクスデータファイル(.mtrx)の書き込み]
                        // 1つ目のフレームはファイルを新規作成して書き込み
                        // 2つ目以降のフレームは追加で書き込む
                        FileMode mode; // ファイル書き込みモード
                        mode = (iFrame == 0) ? FileMode.Create : FileMode.Append;
                        strErrMsg = WriteMtrxFile(matrixData, strMtrxPath, mode);
                        if (strErrMsg != null)
                        {
                            break;
                        }

                        // 進捗のUIを更新
                        UpdateConvertProgress(frameCount, iFrame + 1);
                    }
                    else
                    {  // なぜか最後から数枚目のフレーム読み込みに失敗することがある。
                        img.Dispose();
                        break;
                    }
                }
                // 進捗のUIを更新
                UpdateConvertProgress(frameCount, frameCount);
            }

            return strErrMsg;
        }

        /// <summary>
        /// 画像を左に90度回転
        /// </summary>
        private void RotateLeft(Mat img)
        {
            // 画像を左に90度回転
            Cv2.Transpose(img, img);  // 転置(行列を入れ替え)
            Cv2.Flip(img, img, FlipMode.Y); // Y軸で反転
        }

        /// <summary>
        /// 入力画像から指定した範囲を切り抜く
        /// </summary>
        private Mat CropImage(Mat img, int x, int y, int width, int height)
        {
            // 切り抜き範囲を指定
            OpenCvSharp.CPlusPlus.Rect roi = new Rect(x, y, width, height);

            // 入力画像から指定した範囲を切り抜く
            Mat outImg = new Mat(img, roi);
            img.Dispose();

            return outImg;
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
        byte[,] MakeMatrixData(Mat img)
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
                        color = 0x03;       // Cyan Blue
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
                        matrixData[y, x] = (byte)((byte)(matrixData[y, x] & 0xF0) | color);
                    }
                }
            }

            return matrixData;
        }

        /// <summary>
        /// mtrxファイルの書き込み
        /// </summary>
        string WriteMtrxFile(byte[,] matrixData, string strFilePath, FileMode mode)
        {
            string strErrMsg = null;

            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(strFilePath, mode)))
                {
                    for (int y = 0; y < MTRX_ROWS_SHOW; y++)
                    {
                        for (int x = 0; x < MTRX_COLS; x++)
                        {
                            writer.Write(matrixData[y, x]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                strErrMsg = ex.Message;
            }

            return strErrMsg;
        }

        // 進捗のUIを更新
        void UpdateConvertProgress(int frameCount, int frameNo)
        {
            progressBar_Convert.Invoke((Action)(() =>
            {
                if (frameNo == 1)
                {
                    progressBar_Convert.Minimum = 0;
                    progressBar_Convert.Maximum = frameCount;
                    progressBar_Convert.Value = 0;
                }
                else
                {
                    progressBar_Convert.Value = frameNo;
                }
            }));

            label_ConvertProgress.Invoke((Action)(() =>
            {
                int percent = 100 * frameNo / frameCount;
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
                string strErrMsg = null;
                byte[] allMatrixData = null; // マトリクスデータ

                dlg.Filter = "matrix data file(*.mtrx)|*.mtrx"; // フィルタを設定
                dlg.FilterIndex = 1;                            // フィルタの初期選択インデックス
                dlg.Title = "select matrix data file";          // ダイアログのタイトル
                dlg.RestoreDirectory = true;                    // 終了時にカレントディレクトリを復元
                // ファイル選択ダイアログを表示
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.Enabled = false; // ボタンを無効

                    strErrMsg = await Task.Run(() =>
                    {
                        // 「マトリクスデータクリア」コマンドを送信
                        strErrMsg = Program.PrpJigCmd.SendCmd_ClearMatrix();
                        if (strErrMsg != null)
                        {
                            return strErrMsg;
                        }

                        // mtrxファイルの読み込み
                        try
                        {
                            allMatrixData = File.ReadAllBytes(dlg.FileName);
                        }
                        catch(Exception ex)
                        {
                            strErrMsg = ex.Message;
                            return strErrMsg;
                        }

                        int remainNum = allMatrixData.Length / MTRX_DATA_SIZE; // 残り枚数
                        int sendNum; // 送信枚数
                        int sentNum = 0; // 送信済み枚数
                        while (remainNum > 0) // 残り枚数が0より大きい場合
                        {
                            // 送信枚数を計算
                            if (remainNum > MTRX_SEND_MAX)
                            {
                                sendNum = MTRX_SEND_MAX;
                            }
                            else
                            {
                                sendNum = remainNum;
                            }

                            byte[] sendMatrixData = new byte[MTRX_DATA_SIZE * sendNum]; // 送信枚数分のマトリクスデータ

                            // sendMatrixDataにデータを格納
                            Array.Copy(
                                allMatrixData,              // 全マトリクスデータ(コピー元)
                                sentNum * MTRX_DATA_SIZE,   //コピー開始位置,
                                sendMatrixData,             // 送信枚数分のマトリクスデータ(コピー先)
                                0,                          //コピー先での開始位置(通常は0),
                                sendNum * MTRX_DATA_SIZE    //コピーする要素数
                                );

                            // 「マトリクスデータ更新」コマンドを送信
                            strErrMsg = Program.PrpJigCmd.SendCmd_UpdateMatrix(sendMatrixData);
                            if (strErrMsg == null)
                            {
                                remainNum -= sendNum; // 残り枚数を更新
                                sentNum += sendNum;   // 送信済み枚数を更新 
                            }
                            else
                            {
                                if (strErrMsg != "A failure response was received from the microcontroller. (There is no space in the buffer)")
                                {
                                    return strErrMsg;
                                }
                            }
                        }
                        return strErrMsg;
                    });

                    if (strErrMsg != null)
                    {
                        UI.ShowErrMsg(this, strErrMsg);
                    }

                    this.Enabled = true; // ボタンを有効
                }
            }
        }
    }
}
