// Copyright © 2025 Shiomachi Software. All rights reserved.
#ifndef FRAME_H
#define FRAME_H

// [define]
// 要求フレームのデータ部の最大サイズ
#define FRM_REQ_DATA_MAX_SIZE  (MTRX_RECV_MAX_NUM * MTRX_DATA_SIZE) // フレーム中のデータ部のバッファのサイズ。どのコマンドのデータ部もこのサイズ以下であること。
// 応答フレームのデータ部の最大サイズ
#define FRM_RES_DATA_MAX_SIZE  256

// [列挙体]
// フレームのヘッダ
typedef enum _E_FRM_HEADER {
    FRM_HEADER_REQ = 0xA0,      // 要求
    FRM_HEADER_RES,             // 応答
} E_FRM_HEADER;

// コマンド
typedef enum _E_FRM_CMD {
    CMD_GET_FW_INFO = 0x0001,       // FW情報取得 
    CMD_GET_FW_ERR,                 // FWエラー取得  
    CMD_CLEAR_FW_ERR,               // FWエラークリア 
    CMD_CLEAR_MTRX,                 // マトリクスデータクリアコマンド
    CMD_UPDATE_MTRX                 // マトリクスデータ更新コマンド
} E_FRM_CMD;                        

// フレーム中のエラーコード
typedef enum _E_FRM_ERR {
    FRM_ERR_SUCCESS = 0x0000,    // 成功
    FRM_ERR_DATA_SIZE,           // データサイズが不正  
    FRM_ERR_PARAM,               // 引数が不正
    FRM_ERR_BUF_NOT_ENOUGH,      // バッファに空きがない
    FRM_ERR_I2C_NO_DEVICE        // I2C:address not acknowledged, or, no device present. 
} E_FRM_ERR;

#pragma pack(1)

// [構造体]

// 要求フレーム
typedef struct _ST_FRM_REQ_FRAME {
    UCHAR   header;                     // ヘッダ(1byte)
    USHORT  seqNo;                      // シーケンス番号(2byte)
    USHORT  cmd;                        // コマンド(2byte)
    USHORT  dataSize;                   // データサイズ(2byte)
    UCHAR   aData[FRM_REQ_DATA_MAX_SIZE];   // データ
    USHORT  checksum;                   // チェックサム(2byte)
} ST_FRM_REQ_FRAME;

// 応答フレーム
typedef struct _ST_FRM_RES_FRAME {
    UCHAR   header;                     // ヘッダ(1byte)
    USHORT  seqNo;                      // シーケンス番号(2byte)
    USHORT  cmd;                        // コマンド(2byte)
    USHORT  errCode;                    // エラーコード(2byte) 
    USHORT  dataSize;                   // データサイズ(2byte)
    UCHAR   aData[FRM_RES_DATA_MAX_SIZE];   // データ
    USHORT  checksum;                   // チェックサム(2byte)
} ST_FRM_RES_FRAME;

// USB/無線の受信データ情報
typedef struct _ST_FRM_RECV_DATA_INFO {
    ULONG reqFrmSize ; 		    // 要求フレームの受信済みサイズ
    ULONG recved_dataSize;	    // データサイズ部の受信済みサイズ
    ULONG recved_checksum; 	    // チェックサム部の受信済みサイズ
    ST_FRM_REQ_FRAME stReqFrm; 	// 要求フレーム 
    UCHAR *p;		            // 要求フレームデータ格納先ポインタ
} ST_FRM_RECV_DATA_INFO;

#pragma pack()

// [関数プロトタイプ宣言]
void FRM_Init();
void FRM_RecvMain();
void FRM_MakeAndSendResFrm(USHORT seqNo, USHORT cmd, USHORT errCode, USHORT dataSize, PVOID pBuf);

#endif