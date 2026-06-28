// Copyright © 2025 Shiomachi Software. All rights reserved.
#ifndef COMMON_H
#define COMMON_H

#include "class/cdc/cdc_device.h"
#include "hardware/adc.h"
#include "hardware/clocks.h"
#include "hardware/dma.h"
#include "hardware/exception.h"
#include "hardware/flash.h"
#include "hardware/gpio.h"
#include "hardware/i2c.h"
#include "hardware/pll.h"
#include "hardware/pwm.h"
#include "hardware/resets.h"
#include "hardware/spi.h"
#include "hardware/structs/clocks.h"
#include "hardware/structs/pll.h"
#include "hardware/uart.h"
#include "hardware/watchdog.h"
#include "pico/binary_info.h"
#include "pico/bootrom.h"
#include "pico/i2c_slave.h"
#include "pico/multicore.h"
#include "pico/stdlib.h"
#include "pico/unique_id.h"
#include <stddef.h>
#include <stdio.h>
#include <string.h>

#include "Type.h"
#include "RGBMatrixConfig.h"
#include "Frame.h"
#include "Matrix.h"
#include "Cmd.h"
#include "RGBMatrix_device.h"
#include "Timer.h"
#include "Ver.h"
#include "driver_RGBMatrix.h"
#include "hal_RGBMatrix_device.h"

// [define]
// キューのデータ配列のサイズ
#define CMN_QUE_DATA_MAX_MTRX_RECV (MTRX_RECV_MAX_NUM + 1)

// [列挙体]
// キューの種類
typedef enum _E_CMN_QUE_KIND {
    CMN_QUE_KIND_MTRX_RECV_A = 0, // マトリクスデータ受信A
    CMN_QUE_KIND_MTRX_RECV_B,     // マトリクスデータ受信B
    CMN_QUE_KIND_NUM              // キューの種類の数
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
#ifdef __cplusplus
extern "C" {
#endif

void CMN_EntrySpinLock(ULONG iQue);
void CMN_ExitSpinLock(ULONG iQue);
bool CMN_Enqueue(ULONG iQue, PVOID pData);
PVOID CMN_DequeueWithoutCopy(ULONG iQue);
bool CMN_IsQueueEmpty(ULONG iQue);
void CMN_ClearQueue(ULONG iQue);
bool CMN_Checksum(PVOID pBuf, USHORT expect, ULONG size);
USHORT CMN_CalcChecksum(PVOID pBuf, ULONG size);
void CMN_Init(void);

#ifdef __cplusplus
}
#endif

#endif
