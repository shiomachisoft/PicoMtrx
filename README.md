# PicoMtrx

## 1.概要

- ドットマトリクスLED(WAVESHARE-20591)にmp4ファイルの動画データを表示するPicoのFWとPCアプリです。 

## 2. 動作環境

- マイコン基板：Raspberry Pi Pico 
- ドットマトリクスLED：WAVESHARE-20591
- PC
  - OS：Windows11またはWindows10
    - ※Windows10の場合、.NET Framework 4.X(4.6.2以上)がインストールされている必要がある。
  
## 3. WAVESHARE-20591の仕様

- 64×32ドット
- RGB各2階調   
   
## 4. FWの仕様

- 画面のリフレッシュレート：30Hz

## 5. PCアプリのインストール手順

- ①PicoMtrxAppフォルダをPCの適当なフォルダにコピーする。

## 6. FWの書き込み手順

- ①Pico上の白いボタンを押したままの状態で、PicoとPCをUSBケーブルで接続する。
- ②Windowsが「RPI-RP2」という名前のUSBドライブを認識したら、白いボタンを離す。
- ③PicoMtrx.uf2を「RPI-RP2」ドライブにドラッグする。

## 7. 使い方

- ①WAVESHARE-20591とPCをUSBケーブルで接続する。
- ②WAVESHARE-20591の電源スイッチをONにする。
- ③PicoMtrxApp.exeを起動する。
- ④PicoのCOM番号を選択してから「connect」ボタンを押す。
- ⑤「MTRX」ボタンを押す。
  - ⇒「MTRX」画面が表示される。
- ⑥「Open mtrx file」ボタンを押してからsample.mtrxを選択するとドットマトリクスLEDにサンプル動画が表示される。

## 8. mtrxファイルの作成の仕方

- 「Convert mp4 to mtrx file」を押してからmp4ファイルを選択する。
  - ⇒mp4ファイルと同じフォルダにmtrxファイルが作成される。

※注意
- 4K動画、HD動画、時間の長い動画はmtrxファイルの作成に時間がかかる。
- WAVESHARE-20591のドット数や色の諧調が少ないため、シンプルな絵の動画が推奨される。
