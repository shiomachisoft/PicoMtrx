# PicoMtrx

Firmware for Raspberry Pi Pico and a PC application for displaying mp4 video data on a dot matrix LED (WAVESHARE-20591).

**Demo Video:**<br>
<a href="https://www.youtube.com/watch?v=Xb-uuDCgQQs">
  <img src="https://img.youtube.com/vi/Xb-uuDCgQQs/maxresdefault.jpg" alt="Demo Video" width="600">
</a>

---

## Operating Environment

* **Microcontroller Board:** Raspberry Pi Pico
* **Dot Matrix LED:** WAVESHARE-20591
* **PC (OS):** Windows 11 or Windows 10
  * Requirement: .NET Framework 4.X (4.6.2 or later) must be installed. (Windows 11 meets this requirement by default.)
  * **Note:** .NET 5 and later versions are not supported.

## Specifications

### WAVESHARE-20591 Specifications
* **Resolution:** 64x32 dots
* **Color Depth:** 2 levels per RGB channel
* **Feature:** Pico can be attached directly

### FW (Firmware) Specifications
* **Screen Refresh Rate:** 30Hz

## System Architecture

<img width="921" height="417" alt="image" src="https://github.com/user-attachments/assets/7e56d678-7741-4bc2-bc3d-d42f4e0ef381" />

## Installation Procedure

### 1. PC App Installation
1. Copy the "PicoMtrxApp" folder to any directory on your PC.

### 2. FW Flashing Procedure
1. While holding down the BOOTSEL button on the Pico, connect the Pico and the PC with a USB cable.
2. Release the white button once Windows recognizes the USB drive named "RPI-RP2".
3. Drag and drop the included "PicoMtrx.uf2" file into the "RPI-RP2" drive.

## Usage

1. Attach the Pico to the WAVESHARE-20591.
2. Connect the WAVESHARE-20591 and the PC with a USB cable.
3. Turn ON the power switch of the WAVESHARE-20591.
4. Launch "PicoMtrxApp.exe" on the PC.
5. Select the Pico's COM port number on the screen and click the "connect" button.
6. Click the "Open mtrx file" button and select "sample.mtrx" to display the sample video on the dot matrix LED.

## How to Create an mtrx File

You can create a dedicated video file from your own mp4 video.

1. Click the "Convert mp4 to mtrx file" button in the app and select the mp4 file you want to convert.
2. An ".mtrx" file will be created in the same folder as the mp4 file.

> **Note**
> * Since the WAVESHARE-20591 has limited dot count and color depth, videos with simple graphics are recommended.

## Source Code

The source code for both the FW and the PC app is available.
* **FW:** Written in C and Pico SDK.
* **PC App:** Written in C# using Visual Studio.

## Terms of Use

Please check the following terms of use before using.
* [Check Terms of Use](https://sites.google.com/view/shiomachisoft/%E5%88%A9%E7%94%A8%E8%A6%8F%E7%B4%84)
