// Copyright © 2025 Shiomachi Software. All rights reserved.
#include "Common.h"

// [ファイルスコープ変数]
static ULONG f_iQue = CMN_QUE_KIND_MTRX_RECV_A; // キューのインデックス

// [関数プロトタイプ宣言]
static void CMD_ExecReqCmd_GetFwInfo(ST_FRM_REQ_FRAME *pstReqFrm);
static void CMD_ExecReqCmd_ClearMtrx(ST_FRM_REQ_FRAME *pstReqFrm);
static void CMD_ExecReqCmd_UpdateMtrx(ST_FRM_REQ_FRAME *pstReqFrm);

// 要求コマンドの実行
void CMD_ExecReqCmd(ST_FRM_REQ_FRAME *pstReqFrm)
{   
    switch (pstReqFrm->cmd) 
    {
    // FW情報取得コマンド
    case CMD_GET_FW_INFO:
        CMD_ExecReqCmd_GetFwInfo(pstReqFrm);
        break;                                       
    // マトリクスデータクリアコマンド
    case CMD_CLEAR_MTRX:
        CMD_ExecReqCmd_ClearMtrx(pstReqFrm);
        break;                  
    // マトリクスデータ更新コマンド
    case CMD_UPDATE_MTRX:
        CMD_ExecReqCmd_UpdateMtrx(pstReqFrm);
        break;                  
    default:
        // 無処理
        break;       
    }
}

// FW情報取得コマンドの実行
static void CMD_ExecReqCmd_GetFwInfo(ST_FRM_REQ_FRAME *pstReqFrm)
{
    USHORT dataSize = 0;                // データサイズの期待値
    USHORT errCode = FRM_ERR_SUCCESS;   // エラーコード 
    ST_FW_INFO stFwInfo;                // FW情報
    
    memset(&stFwInfo, 0, sizeof(stFwInfo)); // 常に初期化しておく

    // データサイズをチェック
    if (pstReqFrm->dataSize != dataSize) {
        errCode = FRM_ERR_DATA_SIZE; // データサイズが不正
    }
    else { // 正常系
        strncpy(stFwInfo.szMakerName, MAKER_NAME, sizeof(stFwInfo.szMakerName) - 1); // メーカー名
        strncpy(stFwInfo.szFwName, FW_NAME, sizeof(stFwInfo.szFwName) - 1);           // FW名 
        stFwInfo.fwVer = FW_VER;                      // FWバージョン
        pico_get_unique_board_id(&stFwInfo.board_id); // ユニークボードID サイズ = PICO_UNIQUE_BOARD_ID_SIZE_BYTES       
    }

    // 応答フレームを送信 (エラー時はデータなしで送信)
    if (errCode == FRM_ERR_SUCCESS) {
        FRM_MakeAndSendResFrm(pstReqFrm->seqNo, pstReqFrm->cmd, errCode, sizeof(stFwInfo), &stFwInfo);
    }
    else {
        FRM_MakeAndSendResFrm(pstReqFrm->seqNo, pstReqFrm->cmd, errCode, 0, NULL);
    }
}

// マトリクスデータクリアコマンドの実行
static void CMD_ExecReqCmd_ClearMtrx(ST_FRM_REQ_FRAME *pstReqFrm)
{
    USHORT dataSize = 0;                // データサイズの期待値
    USHORT errCode = FRM_ERR_SUCCESS;   // エラーコード 

    // データサイズをチェック
    if (pstReqFrm->dataSize != dataSize) {
        errCode = FRM_ERR_DATA_SIZE; // データサイズが不正
    }
    else { // 正常系
        // キューのインデックスを初期化
        f_iQue = CMN_QUE_KIND_MTRX_RECV_A;     
        // マトリクスデータのキューを初期化
        MTRX_InitQueue();       
    }

    // 応答フレームを送信        
    FRM_MakeAndSendResFrm(pstReqFrm->seqNo, pstReqFrm->cmd, errCode, 0, NULL); 
}

// マトリクスデータ更新コマンドの実行
static void CMD_ExecReqCmd_UpdateMtrx(ST_FRM_REQ_FRAME *pstReqFrm)
{
    USHORT dataSize = FRM_REQ_DATA_MAX_SIZE; // データサイズの期待値
    USHORT errCode = FRM_ERR_SUCCESS;   // エラーコード 
    ULONG num; // マトリクスデータの枚数
    ULONG i;
    ST_MTRX_DATA *pstMtrxData = (ST_MTRX_DATA*)pstReqFrm->aData;
    
    // データサイズをチェック (サイズ超過、またはマトリクスサイズで割り切れない場合はエラー)
    if ((pstReqFrm->dataSize > dataSize) || (pstReqFrm->dataSize % MTRX_DATA_SIZE != 0)) {
        errCode = FRM_ERR_DATA_SIZE; // データサイズが不正
    }
    else { // 正常系
        if (CMN_IsQueueEmpty(f_iQue)) { 
            // キューが空の場合
            
            num = pstReqFrm->dataSize / MTRX_DATA_SIZE; // マトリクスデータの枚数
            for (i = 0;  i < num; i++) { // マトリクスデータの枚数分繰り返す
                // 1枚分のマトリクスデータをエンキュー
                (void)CMN_Enqueue(f_iQue, &pstMtrxData[i]);
            }           
            // キューのインデックスを変更
            if (f_iQue == CMN_QUE_KIND_MTRX_RECV_A) {
                f_iQue = CMN_QUE_KIND_MTRX_RECV_B;
            }
            else {
                f_iQue = CMN_QUE_KIND_MTRX_RECV_A;
            }            
        }
        else {
            errCode = FRM_ERR_BUF_NOT_ENOUGH; // バッファに空きがない
        }
    }

    // 応答フレームを送信        
    FRM_MakeAndSendResFrm(pstReqFrm->seqNo, pstReqFrm->cmd, errCode, 0, NULL); 
}