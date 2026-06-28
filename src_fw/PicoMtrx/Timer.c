// Copyright © 2025 Shiomachi Software. All rights reserved.
#include "Common.h"

// [define] 
#define TMR_CALLBACK_PERIOD         1     // 1ms定期タイマコールバック周期(ms)
#define TMR_CALLBACK_PERIOD_REFRESH 33333 // 画面リフレッシュレートのタイマコールバック周期(us)
#define TMR_RECV_TIMEOUT            1000  // 要求フレームのヘッダを受信後、TMR_RECV_TIMEOUT[ms]経過しても要求フレームの末尾まで受信してない場合はタイムアウトとする

// [ファイルスコープ変数の宣言]
static repeating_timer_t f_stTimer = {0};           // 1ms定期タイマコールバック登録時に渡すパラメータ
static repeating_timer_t f_stTimer_Refresh = {0};   // 画面リフレッシュレートのタイマコールバック登録時に渡すパラメータ
static volatile ULONG f_timerCnt_recvTimeout = 0;   // 要求フレームのヘッダを受信後、TMR_RECV_TIMEOUT[ms]経過しても要求フレームの末尾まで受信してない場合はタイムアウトとするタイマカウント
static volatile uint64_t f_timerCnt_refresh = 0;    // 画面リフレッシュ回数

// [関数プロトタイプ宣言]
static bool TMR_PeriodicCallback(repeating_timer_t *rt);
static bool TMR_PeriodicCallback_Refresh(repeating_timer_t *rt);

// 1ms定期タイマコールバック
static bool TMR_PeriodicCallback(repeating_timer_t *pstTimer) 
{
    // タイムアウト監視用タイマカウントの更新
    if (f_timerCnt_recvTimeout < TMR_RECV_TIMEOUT) {
        f_timerCnt_recvTimeout++;
    }

    return true; // keep repeating
}

// 画面リフレッシュレートのタイマコールバック
static bool TMR_PeriodicCallback_Refresh(repeating_timer_t *pstTimer) 
{
    f_timerCnt_refresh++;    

    return true; // keep repeating 
}

// 画面リフレッシュ回数の取得
uint64_t TMR_GetRefreshCnt(void)
{
    uint64_t val1, val2;
    do {
        val1 = f_timerCnt_refresh;
        val2 = f_timerCnt_refresh;
    } while (val1 != val2);
    return val1;
}

// 要求フレームのヘッダを受信後、TMR_RECV_TIMEOUT[ms]経過しても要求フレームの末尾まで受信してない場合はタイムアウトとするタイマカウントをクリア
void TMR_ClearRecvTimeout(void)
{
    f_timerCnt_recvTimeout = 0;
}

// 要求フレームのヘッダを受信後、TMR_RECV_TIMEOUT[ms]経過しても要求フレームの末尾まで受信してない場合はタイムアウトとするか否かを取得
bool TMR_IsRecvTimeout(void)
{
    return (f_timerCnt_recvTimeout >= TMR_RECV_TIMEOUT) ? true : false;
}

// タイマーを初期化
void TMR_Init(void)
{
    // 1ms定期タイマコールバックの登録
    add_repeating_timer_ms(TMR_CALLBACK_PERIOD, TMR_PeriodicCallback, NULL, &f_stTimer);
    // 画面リフレッシュレートのタイマコールバックの登録
    add_repeating_timer_us(TMR_CALLBACK_PERIOD_REFRESH, TMR_PeriodicCallback_Refresh, NULL, &f_stTimer_Refresh);
}