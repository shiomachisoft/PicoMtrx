// Copyright © 2025 Shiomachi Software. All rights reserved.
#ifndef TIMER_H
#define TIMER_H

#include "Common.h"

// [関数プロトタイプ宣言] 
uint64_t TMR_GetRefreshCnt();
void TMR_ClearRecvTimeout();
bool TMR_IsRecvTimeout();
void TMR_Init();

#endif