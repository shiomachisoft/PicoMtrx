# PicoMtrx

## 1.概要

- ドットマトリクスLED(WAVESHARE-20591)にmp4ファイルの動画データを表示するPicoのFWとPCアプリです。
- [YouTube](https://www.youtube.com/watch?v=Xb-uuDCgQQs)

## 2. 動作環境

- マイコン基板：Raspberry Pi Pico 
- ドットマトリクスLED：WAVESHARE-20591
- PC
  - OS：Windows11またはWindows10
    - ※.NET Framework 4.X(4.6.2以上)がインストールされている必要がある。(Windows11はデフォルトでこの条件を満たす。)
  
## 3. WAVESHARE-20591の仕様

- 64×32ドット
- RGB各2階調
- Picoを装着できる
   
## 4. FWの仕様

- 画面のリフレッシュレート：30Hz

## 5. PCアプリのインストール手順

- ①PicoMtrxAppフォルダをPCの適当なフォルダにコピーする。

## 6. FWの書き込み手順

- ①Pico上の白いボタンを押したままの状態で、PicoとPCをUSBケーブルで接続する。
- ②Windowsが「RPI-RP2」という名前のUSBドライブを認識したら、白いボタンを離す。
- ③PicoMtrx.uf2を「RPI-RP2」ドライブにドラッグする。

## 7. 使い方

- ①WAVESHARE-20591にPicoを装着する。
- ②WAVESHARE-20591とPCをUSBケーブルで接続する。
- ③WAVESHARE-20591の電源スイッチをONにする。
- ④PicoMtrxApp.exeを起動する。
- ⑤PicoのCOM番号を選択してから「connect」ボタンを押す。
- ⑥「MTRX」ボタンを押す。
  - ⇒「MTRX」画面が表示される。
- ⑦「Open mtrx file」ボタンを押してからsample.mtrxを選択するとドットマトリクスLEDにサンプル動画が表示される。

## 8. mtrxファイルの作成方法

- 「Convert mp4 to mtrx file」を押してからmp4ファイルを選択する。
  - ⇒mp4ファイルと同じフォルダにmtrxファイルが作成される。

※注意
- 4K動画、HD動画、時間の長い動画はmtrxファイルの作成に時間がかかる。
- WAVESHARE-20591のドット数や色の諧調が少ないため、シンプルな絵の動画が推奨される。

## 9. ソースコード
FWとPCアプリの両方ともソースコードを公開しています。  
FWは、C言語とPico SDKで作成しています。
