// Copyright © 2025 Shiomachi Software. All rights reserved.
#include "Common.h"

// [define]
// #define MTRX_DEBUG

#define MTRX_FLUSH_INTERVAL 20 // ダイナミック点灯の間隔(us)
#define MTRX_PWM_CYCLE 5 // PWM周期
#define MTRX_RED_DUTY_PERCENT 20 // 赤の点灯比率 (%)。設定の刻み幅は10%刻み。
#define MTRX_GREEN_DUTY_PERCENT 50 // 緑の点灯比率 (%)。設定の刻み幅は10%刻み。

// [外部変数のextern]
extern unsigned char (*g_display_rgb)[Matrix_COLS];
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

    if (0 == g_CS_cnt) {
        // フレーム間PWM用のカウンタ更新
        static uint32_t pwm_cnt = 0;
        pwm_cnt++;
        if (pwm_cnt >= MTRX_PWM_CYCLE) {
            pwm_cnt = 0;
        }
        // MTRX_RED_DUTY_PERCENT に応じて赤の出力を有効/無効化
        uint32_t threshold = (MTRX_RED_DUTY_PERCENT * MTRX_PWM_CYCLE) / 100;
        if (pwm_cnt < threshold) {
            HAL_RGBMatrixDeviceSetRedDisable(false);
        } else {
            HAL_RGBMatrixDeviceSetRedDisable(true);
        }

        // MTRX_GREEN_DUTY_PERCENT に応じて緑の出力を有効/無効化
        uint32_t green_threshold = (MTRX_GREEN_DUTY_PERCENT * MTRX_PWM_CYCLE) / 100;
        if (pwm_cnt < green_threshold) {
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
                g_display_rgb = (unsigned char (*)[Matrix_COLS])pMtrxData; // デキューしたマトリクスデータ
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
                            // [デバッグ用]
#ifdef MTRX_DEBUG
        uint64_t prevCnt = f_prevRefreshCnt;   // 前回のリフレッシュ回数
        uint64_t curCnt = TMR_GetRefreshCnt(); // 現在のリフレッシュ回数
        if ((curCnt - prevCnt) > 1) {
            // ここに来た場合はリフレッシュレートが30Hzより遅れている
            volatile int a = 0;
        }
#endif

        // [ダイナミック点灯]
        f_displayDevice->Flush(f_displayDevice); // ダイナミック点灯
        busy_wait_us(MTRX_FLUSH_INTERVAL); // ダイナミック点灯の間隔の時間だけ待つ
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
