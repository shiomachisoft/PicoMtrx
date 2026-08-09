// Copyright © 2025 Shiomachi Software. All rights reserved.
#include "Common.h"

// [define]

#define MTRX_PWM_CYCLE 16 // PWM周期
#define MTRX_RED_DUTY_PERCENT 20 // 赤の点灯比率 (%)。設定の刻み幅は MTRX_DUTY_STEP_PERCENT %刻み。
#define MTRX_GREEN_DUTY_PERCENT 50 // 緑の点灯比率 (%)。設定の刻み幅は MTRX_DUTY_STEP_PERCENT %刻み。
#define MTRX_DUTY_STEP_PERCENT  10  // Duty比設定の刻み幅（%）および分散テーブルのインデックス変換係数
#define MTRX_DUTY_PATTERNS_MAX  10  // 分散テーブルの最大インデックス値（100%分）
#define MTRX_GREEN_PHASE_SHIFT  (MTRX_PWM_CYCLE / 2) // 赤と緑のONタイミングの重なりを避けるための緑の位相シフト量（半周期）

// MTRX_DUTY_STEP_PERCENT %刻みのDuty比に対する MTRX_PWM_CYCLE ステップの ON/OFF分散テーブル (0: OFF, 1: ON)
static const uint8_t f_pwm_patterns[MTRX_DUTY_PATTERNS_MAX + 1][MTRX_PWM_CYCLE] = {
    {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, // 0%
    {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0}, // 10%
    {1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0}, // 20%
    {1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1}, // 30%
    {1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1}, // 40%
    {1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0}, // 50%
    {1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 0}, // 60%
    {1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0}, // 70%
    {1, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1}, // 80%
    {1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1}, // 90%
    {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}  // 100%
};

// [外部変数のextern]
extern ST_COLOR_RGB888 (*g_display_rgb)[Matrix_COLS];
extern uint8_t g_CS_cnt;

// [ファイルスコープ変数]
static PDisplayDevice f_displayDevice;          // ディスプレイ
static ULONG f_iQue = CMN_QUE_KIND_MTRX_RECV_A; // キューのインデックス
static ULONG f_dequeueCnt = 0;                  // デキュー回数
static bool f_isDequeuedOnce = false; // 一度でもデキューしたことがあるか否か
static uint64_t f_prevRefreshCnt = 0; // 前回のリフレッシュ回数

// LEDマトリクスのメイン処理
void MTRX_Main(void) {
    uint64_t refreshCnt; // 現在のリフレッシュ回数
    PVOID pMtrxData;     // マトリクスデータ

    if (((Matrix_ROWS_SHOW - 1) == g_CS_cnt) || !f_isDequeuedOnce) {
        // フレーム間PWM用のカウンタ更新
        static uint32_t pwm_cnt = 0;
        pwm_cnt++;
        if (pwm_cnt >= MTRX_PWM_CYCLE) {
            pwm_cnt = 0;
        }

        // 分散テーブルに基づき赤の有効/無効化を決定
        uint32_t red_idx = MTRX_RED_DUTY_PERCENT / MTRX_DUTY_STEP_PERCENT;
        if (red_idx > MTRX_DUTY_PATTERNS_MAX) red_idx = MTRX_DUTY_PATTERNS_MAX;
        if (f_pwm_patterns[red_idx][pwm_cnt]) {
            HAL_RGBMatrixDeviceSetRedDisable(false);
        } else {
            HAL_RGBMatrixDeviceSetRedDisable(true);
        }

        // 分散テーブルに基づき緑の有効/無効化を決定
        // 赤と緑のONタイミングの重なりを避けるため、緑は位相を半周期（MTRX_GREEN_PHASE_SHIFT）ずらす
        uint32_t green_idx = MTRX_GREEN_DUTY_PERCENT / MTRX_DUTY_STEP_PERCENT;
        if (green_idx > MTRX_DUTY_PATTERNS_MAX) green_idx = MTRX_DUTY_PATTERNS_MAX;
        uint32_t green_pwm_cnt = (pwm_cnt + MTRX_GREEN_PHASE_SHIFT) % MTRX_PWM_CYCLE;
        if (f_pwm_patterns[green_idx][green_pwm_cnt]) {
            HAL_RGBMatrixDeviceSetGreenDisable(false);
        } else {
            HAL_RGBMatrixDeviceSetGreenDisable(true);
        }

        // 現在のリフレッシュ回数を取得
        refreshCnt = TMR_GetRefreshCnt();
        if ((0 == f_prevRefreshCnt) || (f_prevRefreshCnt != refreshCnt)) {
            // リフレッシュ回数が更新されている場合

            // マトリクスデータをデキュー(コピー無し)
            pMtrxData = CMN_DequeueWithoutCopy(f_iQue);
            if (NULL != pMtrxData) {
                g_display_rgb = (ST_COLOR_RGB888 (*)[Matrix_COLS])pMtrxData; // デキューしたマトリクスデータ
                f_isDequeuedOnce = true; // 一度でもデキューした
                f_dequeueCnt++;          // デキュー回数
                if (f_dequeueCnt >= MTRX_RECV_MAX_NUM) { // デキュー回数 >= キューイングできる最大枚数
                     f_dequeueCnt = 0;      // デキュー回数をリセット
                    // デキューするキューを変更する
                    if (f_iQue == CMN_QUE_KIND_MTRX_RECV_A) {
                        f_iQue = CMN_QUE_KIND_MTRX_RECV_B;
                    } else {
                        f_iQue = CMN_QUE_KIND_MTRX_RECV_A;
                    }
                }
            }
            f_prevRefreshCnt = refreshCnt; // 前回のリフレッシュカウントを更新
        }
    }

    if (f_isDequeuedOnce) { // 一度でもデキューしたことがある場合
        // [ダイナミック点灯]
        f_displayDevice->Flush(f_displayDevice); // ダイナミック点灯 (関数内で点灯時間制御と消灯まで実行されます)
    }
}

// マトリクスデータのキューを初期化
void MTRX_InitQueue(void) {
    f_iQue = CMN_QUE_KIND_MTRX_RECV_A;
    f_dequeueCnt = 0;
    CMN_ClearQueue(CMN_QUE_KIND_MTRX_RECV_A);
    CMN_ClearQueue(CMN_QUE_KIND_MTRX_RECV_B);
}

// LEDマトリクスを初期化
void MTRX_Init(void) {
    // ディスプレイを初期化
    f_displayDevice = GetDisplayDevice();
    f_displayDevice->Init();
    memset(f_displayDevice->FBBase, 0xff, Matrix_ROWS * Matrix_COLS_BYTE);
}
