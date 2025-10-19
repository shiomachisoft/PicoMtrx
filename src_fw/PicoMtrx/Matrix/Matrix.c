// Copyright © 2025 Shiomachi Software. All rights reserved.
#include "Common.h"

// [define]
//#define MTRX_DEBUG

#define MTRX_FLUSH_INTERVAL 1 // ダイナミック点灯の間隔(us)

// [外部変数のextern]
//extern unsigned char display_rgb[Matrix_ROWS_SHOW][Matrix_COLS];
extern unsigned char (*display_rgb)[Matrix_COLS];
extern uint8_t CS_cnt;

// [ファイルスコープ変数]
static PDisplayDevice f_displayDevice; // ディスプレイ
static ULONG f_iQue = CMN_QUE_KIND_MTRX_RECV_A; // キューのインデックス
static ULONG f_dequeueCnt = 0; // デキュー回数
static bool f_isDequeuedOnce = false; // 一度でもデキューしたことがあるか否か
static uint64_t s_prevRefreshCnt = 0; // 前回のリフレッシュ回数

// LEDマトリクスのメイン処理
void MTRX_Main()
{ 
    uint64_t refreshCnt; // 現在のリフレッシュ回数
    PVOID pMtrxData; // マトリクスデータ

    if (0 == CS_cnt) {	
        // 現在のリフレッシュ回数を取得
        refreshCnt = TMR_GetRefreshCnt();
		if ((0 == s_prevRefreshCnt) || (s_prevRefreshCnt != refreshCnt)) {
            // リフレッシュ回数が更新されている場合    

            // マトリクスデータをデキュー(コピー無し)
            pMtrxData = CMN_DequeueWithoutCopy(f_iQue);
            if (NULL != pMtrxData) {
                display_rgb = (unsigned char (*)[Matrix_COLS])pMtrxData; // デキューしたマトリクスデータ
                f_isDequeuedOnce = true;  // 一度でもデキューした
                f_dequeueCnt++; // デキュー回数
                if (f_dequeueCnt >= MTRX_RECV_MAX_NUM) { // デキュー回数 >= キューイングできる最大枚数
                    f_dequeueCnt = 0; // デキュー回数をリセット
                    // デキューするキューを変更する
                    if (f_iQue == CMN_QUE_KIND_MTRX_RECV_A) {
                        f_iQue = CMN_QUE_KIND_MTRX_RECV_B;
                    }
                    else {
                        f_iQue = CMN_QUE_KIND_MTRX_RECV_A;
                    }
                }	
            }
            s_prevRefreshCnt = refreshCnt; // 前回のリフレッシュカウントを更新
		}
    }         

    if (f_isDequeuedOnce) { // 一度でもデキューしたことがある場合
        // [デバッグ用]
#ifdef MTRX_DEBUG
        uint64_t prevCnt = s_prevRefreshCnt;    // 前回のリフレッシュ回数
        uint64_t curCnt = TMR_GetRefreshCnt(); // 現在のリフレッシュ回数   
        if ((curCnt - prevCnt) > 1) {
            volatile int a = 0; // ここ来た場合はリフレッシュレートが30Hzより遅れている
        } 
#endif
        // <=====

        // [ダイナミック点灯]    
        f_displayDevice->Flush(f_displayDevice); // ダイナミック点灯
        busy_wait_us(MTRX_FLUSH_INTERVAL);   // ダイナミック点灯の間隔の時間だけ待つ       
    }
}

// マトリクスデータのキューを初期化
void MTRX_InitQueue()
{
    f_iQue = CMN_QUE_KIND_MTRX_RECV_A; 
    f_dequeueCnt = 0;    
    CMN_ClearQueue(CMN_QUE_KIND_MTRX_RECV_A);
    CMN_ClearQueue(CMN_QUE_KIND_MTRX_RECV_B);   
}

// LEDマトリクスを初期化
void MTRX_Init()
{  
    // ディスプレイを初期化
    f_displayDevice = GetDisplayDevice();
	f_displayDevice->Init();
    memset(f_displayDevice->FBBase, 0xff, 256);
}














