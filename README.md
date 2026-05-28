# PicoMtrx

ドットマトリクスLED (WAVESHARE-20591) にmp4ファイルの動画データを表示するための、Raspberry Pi Pico用ファームウェア（FW）とPC向けアプリです。

🎥 **デモ動画:** [YouTube](https://www.youtube.com/watch?v=Xb-uuDCgQQs)

---

## 動作環境

*   **マイコン基板:** Raspberry Pi Pico
*   **ドットマトリクスLED:** WAVESHARE-20591
*   **PC (OS):** Windows 11 または Windows 10
    *   ※必須要件: .NET Framework 4.X (4.6.2以上) がインストールされていること。（Windows 11は標準でこの条件を満たしています）。
    *   **※注意:** .NET 5以降はサポート対象外となりますのでご注意ください。

## 仕様

### WAVESHARE-20591 の仕様
*   **解像度:** 64×32ドット
*   **色表現:** RGB各2階調
*   **特徴:** Picoを直接装着可能

### FW（ファームウェア）の仕様
*   **画面リフレッシュレート:** 30Hz

## システム構成

<img width="1076" height="488" alt="システム構成図" src="https://github.com/user-attachments/assets/164809ec-3949-4ce5-90fa-d0c543b1059d" />

---

## 導入手順

### 1. PCアプリのインストール
1. `PicoMtrxApp` フォルダを、PC内の任意のフォルダにコピーします。

### 2. FWの書き込み手順
1. PicoのBOOTSELボタンを押したままの状態で、PicoとPCをUSBケーブルで接続します。
2. Windowsが「RPI-RP2」という名前のUSBドライブを認識したら、白いボタンを離します。
3. 同梱の `PicoMtrx.uf2` を、「RPI-RP2」ドライブにドラッグ＆ドロップします。

---

## 使い方

1. WAVESHARE-20591にPicoを装着します。
2. WAVESHARE-20591とPCをUSBケーブルで接続します。
3. WAVESHARE-20591の電源スイッチをONにします。
4. PC側で `PicoMtrxApp.exe` を起動します。
5. 画面上でPicoのCOM番号を選択し、「connect」ボタンを押します。
6. 「MTRX」ボタンを押すと、「MTRX」画面が表示されます。
7. 「Open mtrx file」ボタンを押してから `sample.mtrx` を選択すると、ドットマトリクスLEDにサンプル動画が表示されます。

---

## mtrxファイルの作成方法

ご自身のmp4動画から専用の動画ファイルを作成できます。

1. アプリ上で「Convert mp4 to mtrx file」ボタンを押し、変換したいmp4ファイルを選択します。
2. mp4ファイルと同じフォルダ内に `.mtrx` ファイルが作成されます。

> **⚠️ 注意事項**
> *   4K動画、HD動画、または時間の長い動画は、mtrxファイルの作成処理に時間がかかります。
> *   WAVESHARE-20591はドット数や色の階調が限られているため、シンプルな絵の動画を推奨します。

---

## ソースコード

FWとPCアプリ、両方のソースコードを公開しています。
*   **FW:** C言語 および Pico SDK で作成。
*   **PCアプリ:** Visual Studio にて C# で作成。

## 利用規約

ご使用前に以下の利用規約をご確認ください。
*   [利用規約を確認する](https://sites.google.com/view/shiomachisoft/%E5%88%A9%E7%94%A8%E8%A6%8F%E7%B4%84)
