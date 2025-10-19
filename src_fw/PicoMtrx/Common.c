// Copyright © 2025 Shiomachi Software. All rights reserved.
#include "Common.h" 

// [ファイルスコープ変数]
static critical_section_t f_astSpinLock[CMN_QUE_KIND_NUM] = {0}; // スピンロック
static ST_QUE f_aQue[CMN_QUE_KIND_NUM] = {0}; // キューの配列
static ST_MTRX_DATA f_aQueData_mtrxRecv_A[CMN_QUE_DATA_MAX_MTRX_RECV] = {0}; // マトリクスデータ受信Aキューのデータ配列
static ST_MTRX_DATA f_aQueData_mtrxRecv_B[CMN_QUE_DATA_MAX_MTRX_RECV] = {0}; // マトリクスデータ受信Bキューのデータ配列

// [関数プロトタイプ宣言]

// エンキュー
bool CMN_Enqueue(ULONG iQue, PVOID pData) 
{
	bool bRet = false;
	ST_QUE *pstQue = &f_aQue[iQue];
	ST_MTRX_DATA* pstMtrxData;

	CMN_EntrySpinLock(iQue); // スピンロックを獲得

	if ((pstQue->head == (pstQue->tail + 1) % pstQue->max)) { 
		// キューが満杯の場合
		
		// 無処理
	}
	else {
		// キューイング
		switch (iQue) {
		case CMN_QUE_KIND_MTRX_RECV_A:	// マトリクスデータ受信A 
		case CMN_QUE_KIND_MTRX_RECV_B:	// マトリクスデータ受信B 
			pstMtrxData = (ST_MTRX_DATA*)pstQue->pBuf;
			memcpy(&pstMtrxData[pstQue->tail], pData, sizeof(ST_MTRX_DATA));
			break; 
		default:
			// ここに来ない
			break;				
		}	
		pstQue->tail = (pstQue->tail + 1) % pstQue->max;
		bRet = true;
	}

	CMN_ExitSpinLock(iQue); // スピンロックを解放

	return bRet;
}

// デキュー
bool CMN_Dequeue(ULONG iQue, PVOID pData)
{
	bool bRet = false;
	ST_QUE *pstQue = &f_aQue[iQue];	
	ST_MTRX_DATA* pstMtrxData;
	
	CMN_EntrySpinLock(iQue); // スピンロックを獲得

    if (pstQue->head == pstQue->tail) {
		// キューが空の場合

		// 無処理
	}
	else {
		// デキュー
		switch (iQue) {
		case CMN_QUE_KIND_MTRX_RECV_A:	// マトリクスデータ受信A
		case CMN_QUE_KIND_MTRX_RECV_B:	// マトリクスデータ受信B 		
			pstMtrxData = (ST_MTRX_DATA*)pstQue->pBuf;
			memcpy(pData, &pstMtrxData[pstQue->head], sizeof(ST_MTRX_DATA));
			break; 
		default:
			// ここに来ない
			break;				
		}	
		pstQue->head = (pstQue->head + 1) % pstQue->max;
		bRet = true;
	}

	CMN_ExitSpinLock(iQue); // スピンロックを解放

    return bRet;
}

// デキュー(コピー無し)
PVOID CMN_DequeueWithoutCopy(ULONG iQue)
{
	PVOID pData = NULL;
	ST_QUE *pstQue = &f_aQue[iQue];	
	ST_MTRX_DATA* pstMtrxData = NULL;

	CMN_EntrySpinLock(iQue); // スピンロックを獲得

    if (pstQue->head == pstQue->tail) {
		// キューが空の場合

		// 無処理
	}
	else {
		// デキュー
		switch (iQue) {
		case CMN_QUE_KIND_MTRX_RECV_A:	// マトリクスデータ受信A
		case CMN_QUE_KIND_MTRX_RECV_B:	// マトリクスデータ受信B
			pstMtrxData = (ST_MTRX_DATA*)pstQue->pBuf; 		
			pData = (PVOID)&pstMtrxData[pstQue->head];
			break; 
		default:
			// ここに来ない
			break;				
		}	
		pstQue->head = (pstQue->head + 1) % pstQue->max;
	}

	CMN_ExitSpinLock(iQue); // スピンロックを解放	

	return pData;
}

// キューが空か否かを取得
bool CMN_IsQueueEmpty(ULONG iQue)
{
	bool bRet = false;
	ST_QUE *pstQue = &f_aQue[iQue];	

	CMN_EntrySpinLock(iQue); // スピンロックを獲得
	
	if (pstQue->head == pstQue->tail) { // キューが空の場合
		bRet = true;
	}

	CMN_ExitSpinLock(iQue); // スピンロックを解放	

	return bRet;
}

// キューを空にする
void CMN_ClearQueue(ULONG iQue)
{
	ST_QUE *pstQue = &f_aQue[iQue];	

	CMN_EntrySpinLock(iQue); // スピンロックを獲得
	
	pstQue->head = 0;
	pstQue->tail = 0;

	CMN_ExitSpinLock(iQue); // スピンロックを解放	
}

// スピンロックを獲得
// スピンロックはCPU間排他をしつつ割り込みを禁止にする場合に使用する。
// CPU間排他だけならミューテックスを使用すること。
// Picoのcritical_section(spin lock)とmutexの定義は下記。
// https://www.raspberrypi.com/documentation/pico-sdk/high_level.html#pico_sync
// critical_section(spin lock):
// Critical Section API for short-lived mutual exclusion safe for IRQ and multi-core. 
// mutex:
// Mutex API for non IRQ mutual exclusion between cores. 
void CMN_EntrySpinLock(ULONG iQue)
{
	critical_section_enter_blocking(&f_astSpinLock[iQue]);
}

// スピンロックを解放
void CMN_ExitSpinLock(ULONG iQue)
{
	critical_section_exit(&f_astSpinLock[iQue]);
}

// チェックサム検査を実行
bool CMN_Checksum(PVOID pBuf, USHORT expect, ULONG size)
{
	UCHAR* pDataAry = (UCHAR*)pBuf;
	USHORT checksum = 0;
	ULONG i;
	bool bRet = false;

	// チェックサムの値を計算
	for (i = 0; i < size; i++) {
		checksum += pDataAry[i];
	}

	// チェックサム値のチェック
	if (checksum == expect) {
		bRet = true;				
	}

	return bRet;	
}

// チェックサムを計算
USHORT CMN_CalcChecksum(PVOID pBuf, ULONG size)
{
	UCHAR* pDataAry = (UCHAR*)pBuf;
	USHORT checksum = 0;
	ULONG i;

	for (i = 0; i < size; i++) {
		checksum += pDataAry[i];			
	}

	return checksum;
}

// 共通ライブラリを初期化
void CMN_Init()
{
	ULONG i;

	// [変数を初期化]
	for (i = 0; i < CMN_QUE_KIND_NUM; i++) {
		critical_section_init(&f_astSpinLock[i]);
	}
	f_aQue[CMN_QUE_KIND_MTRX_RECV_A].pBuf = (PVOID)f_aQueData_mtrxRecv_A;
	f_aQue[CMN_QUE_KIND_MTRX_RECV_A].max = CMN_QUE_DATA_MAX_MTRX_RECV;
	f_aQue[CMN_QUE_KIND_MTRX_RECV_B].pBuf = (PVOID)f_aQueData_mtrxRecv_B;
	f_aQue[CMN_QUE_KIND_MTRX_RECV_B].max = CMN_QUE_DATA_MAX_MTRX_RECV;
}