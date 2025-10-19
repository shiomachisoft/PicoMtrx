// Copyright © 2025 Shiomachi Software. All rights reserved.
#ifndef MATRIX_H
#define MATRIX_H

#include "Common.h"

// [構造体]
// マトリクスデータ構造体
typedef struct _ST_MTRX_DATA {
    UCHAR aData[MTRX_DATA_SIZE];
} ST_MTRX_DATA;

// [関数プロトタイプ宣言]
void MTRX_Init();
void MTRX_Main();
void MTRX_InitQueue();

#endif