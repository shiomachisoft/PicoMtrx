// Copyright © 2025 Shiomachi Software. All rights reserved.
#ifndef COMMON_H
#define COMMON_H

#include <stdio.h>
#include <string.h>
#include <stddef.h>
#include "pico/stdlib.h"
#include "pico/binary_info.h"
#include "hardware/gpio.h"
#include "hardware/adc.h"
#include "hardware/uart.h"
#include "hardware/spi.h"
#include "hardware/dma.h"
#include "hardware/i2c.h"
#include "pico/i2c_slave.h"
#include "hardware/pwm.h"
#include "pico/multicore.h"
#include "hardware/flash.h"
#include "class/cdc/cdc_device.h"
#include "pico/unique_id.h"
#include "hardware/pll.h"
#include "hardware/clocks.h"
#include "hardware/structs/pll.h"
#include "hardware/structs/clocks.h"
#include "hardware/watchdog.h"
#include "hardware/resets.h"
#include "pico/bootrom.h"
#include "hardware/exception.h"

#include "Type.h"
#include "Ver.h"
#include "RGBMatrixConfig.h"
#include "RGBMatrix_device.h"
#include "hal_RGBMatrix_device.h"
#include "driver_RGBMatrix.h"
#include "Matrix.h"
#include "Frame.h"
#include "Timer.h"
#include "Cmd.h"

// [define]
// キューのデータ配列のサイズ
#define CMN_QUE_DATA_MAX_MTRX_RECV    (MTRX_RECV_MAX_NUM + 1) 

// FWエラービット
#define CMN_ERR_BIT_WDT_RESET                       (1 << 0)  // WDTタイムアウトでマイコンがリセットした
#define CMN_ERR_BIT_UART_OVERRUN_ERR                (1 << 1)  // UART:Framing error     
#define CMN_ERR_BIT_UART_BREAK_ERR                  (1 << 2)  // UART:Parity error
#define CMN_ERR_BIT_UART_PARITY_ERR                 (1 << 3)  // UART:Break error
#define CMN_ERR_BIT_UART_FRAMING_ERR                (1 << 4)  // UART:Overrun error
#define CMN_ERR_I2C_NO_DEVICE                       (1 << 5)  // I2C:address not acknowledged, or, no device present.(PICO_ERROR_GENERICの意味)
#define CMN_ERR_I2C_TIMEOUT                         (1 << 6)  // I2C通信でタイムアウト
#define CMN_ERR_BIT_BUF_SIZE_NOT_ENOUGH_USB_WL_SEND (1 << 7)  // バッファに空きがないので要求データを破棄した(USB/無線送信)
#define CMN_ERR_BIT_BUF_SIZE_NOT_ENOUGH_UART_SEND   (1 << 8)  // バッファに空きがないので要求データを破棄した(UART送信)
#define CMN_ERR_BIT_BUF_SIZE_NOT_ENOUGH_UART_RECV   (1 << 9)  // バッファに空きがないので要求データを破棄した(UART受信)
#define CMN_ERR_BIT_BUF_SIZE_NOT_ENOUGH_I2C_REQ     (1 << 10) // バッファに空きがないので要求データを破棄した(I2C送信/受信)
#define CMN_ERR_BIT_BUF_SIZE_NOT_ENOUGH_WL_RECV     (1 << 11) // バッファに空きがないので要求データを破棄した(無線受信)
#define CMN_ERR_BIT_WL_SEND_ERR                     (1 << 12) // 無線送信が失敗した

// [列挙体]
// キューの種類
typedef enum _E_CMN_QUE_KIND { 
    CMN_QUE_KIND_MTRX_RECV_A = 0,   // マトリクスデータ受信A
    CMN_QUE_KIND_MTRX_RECV_B,       // マトリクスデータ受信B
    CMN_QUE_KIND_NUM                // キューの種類の数
} E_CMN_QUE_KIND;

#pragma pack(1)

// [構造体]
// キュー
typedef struct _ST_QUE {
    ULONG head; // 先頭
    ULONG tail; // 末尾
    ULONG max;  // キューのデータ配列の要素数
    PVOID pBuf; // キューのデータ配列へのポインタ
} ST_QUE;

#pragma pack()

// [関数プロトタイプ宣言]
void CMN_EntrySpinLock(ULONG iQue);
void CMN_ExitSpinLock(ULONG iQue);
bool CMN_Enqueue(ULONG iQue, PVOID pData);
bool CMN_Dequeue(ULONG iQue, PVOID pData);
PVOID CMN_DequeueWithoutCopy(ULONG iQue);
bool CMN_IsQueueEmpty(ULONG iQue);
void CMN_ClearQueue(ULONG iQue);
bool CMN_Checksum(PVOID pBuf, USHORT expect, ULONG size);
USHORT CMN_CalcChecksum(PVOID pBuf, ULONG size);
void CMN_Init();

#endif

