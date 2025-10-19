// Copyright © 2025 Shiomachi Software. All rights reserved.
#include "Common.h"

// [関数プロトタイプ宣言]
static void MAIN_Init();
static void MAIN_MainLoop_Core0();
static void MAIN_MainLoop_Core1();

// メイン関数
int main(void)
{
	// 電源起動時の初期化
	MAIN_Init();

	// CPUコア1のメインループを開始
	multicore_launch_core1(MAIN_MainLoop_Core1); 

	// CPUコア0のメインループを開始
	MAIN_MainLoop_Core0();

	return 0;
}

// CPUコア0のメインループ
static void MAIN_MainLoop_Core0()
{
    while (1) 
	{
		// USB受信データ取り出し⇒コマンド解析・実行
		FRM_RecvMain();
    }
}

// CPUコア1のメインループ
static void MAIN_MainLoop_Core1() 
{
	while (1) 
	{
		// LEDマトリクスのメイン処理 
		MTRX_Main();
	}
}

// 電源起動時の初期化
static void MAIN_Init()
{
	// CDCを初期化
    stdio_init_all();	
	// 共通ライブラリを初期化
	CMN_Init();
	// LEDマトリクスを初期化
	MTRX_Init();
	// USB通信を初期化
	FRM_Init();
	// タイマーを初期化
	TMR_Init();
}