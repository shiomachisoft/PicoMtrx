#ifndef __RGBMATRIXCONFIG_H
#define __RGBMATRIXCONFIG_H

#define CN 0
#define EN 1

#define Matrix_COLS 64
#define Matrix_ROWS 32

#define Matrix_ROWS_SHOW (Matrix_ROWS/2)
#define Matrix_COLS_BYTE (Matrix_COLS/8)

#define MTRX_DATA_SIZE (Matrix_ROWS_SHOW * Matrix_COLS) // マトリクスデータサイズ
#define MTRX_RECV_MAX_NUM 30 // マトリクスデータ更新コマンドで受信する最大枚数 = キューイングできる最大枚数

#endif
