#ifndef __RGBMATRIXCONFIG_H
#define __RGBMATRIXCONFIG_H

#define Matrix_COLS 64
#define Matrix_ROWS 32

#define Matrix_ROWS_SHOW (Matrix_ROWS/2)
#define Matrix_COLS_BYTE (Matrix_COLS/8)

#define MTRX_DATA_SIZE (Matrix_ROWS * Matrix_COLS * 3) // マトリクスデータサイズ (Matrix_ROWS * Matrix_COLS * 3 バイト)
#define MTRX_RECV_MAX_NUM 10 // マトリクスデータ更新コマンドで受信する最大枚数 = キューイングできる最大枚数
#define MTRX_ACTIVE_DURATION_US 10 // 電圧降下による色崩れ（黄色化）を防ぐための1行あたりの最大点灯時間(us)

#endif
