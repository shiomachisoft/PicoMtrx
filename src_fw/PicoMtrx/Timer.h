// Copyright © 2025 Shiomachi Software. All rights reserved.
#ifndef TIMER_H
#define TIMER_H

#include "Common.h"

// [関数プロトタイプ宣言] 
#ifdef __cplusplus
extern "C" {
#endif

uint64_t TMR_GetRefreshCnt(void);
void TMR_ClearRecvTimeout(void);
bool TMR_IsRecvTimeout(void);
void TMR_Init(void);

#ifdef __cplusplus
}
#endif

#endif