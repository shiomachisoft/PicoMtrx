#include "Common.h"
#include "hardware/pio.h"
#include "rgb_matrix.pio.h"

#define MTRX_INIT_CLK_WAIT_US   2  // 初期化レジスタ送信時のクロックパルス幅確保のためのウェイト時間(us)
#define MTRX_REG12_WRITE_CLK_COUNT  12 // レジスタ12の書き込みに必要なSTBパルスHigh幅クロック数
#define MTRX_REG13_WRITE_CLK_COUNT  13 // レジスタ13の書き込みに必要なSTBパルスHigh幅クロック数
#define MTRX_STB_PULSE_WIDTH_US     1  // ストローブ（ラッチ）パルスの幅(us)

// PIOに送信するデータ内での各カラーチャンネルのビット位置定義
#define MTRX_BIT_R1                 0  // 赤1(上半分)のビット位置
#define MTRX_BIT_G1                 1  // 緑1(上半分)のビット位置
#define MTRX_BIT_B1                 2  // 青1(上半分)のビット位置
#define MTRX_BIT_R2                 3  // 赤2(下半分)のビット位置
#define MTRX_BIT_G2                 6  // 緑2(下半分)のビット位置
#define MTRX_BIT_B2                 7  // 青2(下半分)のビット位置

// PIOへのデータパッキング用定数
#define MTRX_PIXELS_PER_WORD        4  // 1ワード(32ビット)あたりにパッキングするピクセル数
#define MTRX_PIXEL_DATA_WIDTH       8  // 1ピクセルデータのビット幅(1バイト=8ビット)
#define MTRX_PIXELS_PER_ITER        8  // 1回のループイテレーションで処理する合計ピクセル数(word1 + word2分)

extern "C" {
ST_COLOR_RGB888 (*g_display_rgb)[Matrix_COLS];
uint8_t g_CS_cnt = 0;
bool g_disable_red = false;
bool g_disable_green = false;
}

static PIO g_pio;
static uint g_sm;

void picoRGBMatrixDeviceInit(void)
{
    gpio_init(R1);
    gpio_init(G1);
    gpio_init(B1);
    gpio_init(R2);
    gpio_init(G2);
    gpio_init(B2);

    gpio_init(A);
    gpio_init(B);
    gpio_init(C);
    gpio_init(D);
    gpio_init(E);

    gpio_init(CLK);
    gpio_init(STB);
    gpio_init(OE);

    gpio_set_dir(R1, GPIO_OUT);
    gpio_set_dir(G1, GPIO_OUT);
    gpio_set_dir(B1, GPIO_OUT);
    gpio_set_dir(R2, GPIO_OUT);
    gpio_set_dir(G2, GPIO_OUT);
    gpio_set_dir(B2, GPIO_OUT);

    gpio_set_dir(A, GPIO_OUT);
    gpio_set_dir(B, GPIO_OUT);
    gpio_set_dir(C, GPIO_OUT);
    gpio_set_dir(D, GPIO_OUT);
    gpio_set_dir(E, GPIO_OUT);
    gpio_set_dir(CLK, GPIO_OUT);
    gpio_set_dir(STB, GPIO_OUT);
    gpio_set_dir(OE, GPIO_OUT);

    OE_HIGH;
    STB_LOW;
    CLK_LOW;

    int MaxLed = Matrix_COLS;

    int C12[Matrix_ROWS_SHOW] = {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1};
    int C13[Matrix_ROWS_SHOW] = {0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0};

    // 制御レジスタ12へデータを送信
    for (int l = 0; l < MaxLed; l++)
    {
        int y = l % Matrix_ROWS_SHOW;
        R1_LOW;
        G1_LOW;
        B1_LOW;
        R2_LOW;
        G2_LOW;
        B2_LOW;
        if (C12[y] == 1)
        {
            R1_HIGH;
            G1_HIGH;
            B1_HIGH;
            R2_HIGH;
            G2_HIGH;
            B2_HIGH;
        }
        if (l >= MaxLed - MTRX_REG12_WRITE_CLK_COUNT)
        {
            STB_HIGH;
        }
        else
        {
            STB_LOW;
        }
        CLK_HIGH;
        busy_wait_us(MTRX_INIT_CLK_WAIT_US);
        CLK_LOW;
        busy_wait_us(MTRX_INIT_CLK_WAIT_US);
    }
    STB_LOW;
    CLK_LOW;

    // 制御レジスタ13へデータを送信
    for (int l = 0; l < MaxLed; l++)
    {
        int y = l % Matrix_ROWS_SHOW;
        R1_LOW;
        G1_LOW;
        B1_LOW;
        R2_LOW;
        G2_LOW;
        B2_LOW;
        if (C13[y] == 1)
        {
            R1_HIGH;
            G1_HIGH;
            B1_HIGH;
            R2_HIGH;
            G2_HIGH;
            B2_HIGH;
        }
        if (l >= MaxLed - MTRX_REG13_WRITE_CLK_COUNT)
        {
            STB_HIGH;
        }
        else
        {
            STB_LOW;
        }
        CLK_HIGH;
        busy_wait_us(MTRX_INIT_CLK_WAIT_US);
        CLK_LOW;
        busy_wait_us(MTRX_INIT_CLK_WAIT_US);
    }
    STB_LOW;
    CLK_LOW;

    // マトリクスデータのクロック制御を行うPIOステートマシンを初期化
    g_pio = pio0;
    g_sm = pio_claim_unused_sm(g_pio, true);
    uint offset = pio_add_program(g_pio, &rgb_matrix_program);
    rgb_matrix_program_init(g_pio, g_sm, offset, R1, 8, CLK);

    // 初期化時のゴミ表示を防ぐため、1行分の消灯データ（すべて0）をシフトレジスタに送っておく
    for (int i = 0; i < Matrix_ROWS_SHOW; i++) {
        pio_sm_put_blocking(g_pio, g_sm, 0);
    }
    // 送信完了（ストール）を待つ
    uint32_t stall_mask = 1u << (PIO_FDEBUG_TXSTALL_LSB + g_sm);
    g_pio->fdebug = stall_mask;
    while (!(g_pio->fdebug & stall_mask)) {
        tight_loop_contents();
    }
}

void RGBMatrixDeviceFlush(uint8_t *buf)
{
    static uint8_t pwm_step = 0;

    // 前の行の点灯完了なので、一旦消灯する（ゴースト防止）
    OE_HIGH;

    // 今回表示する行（g_CS_cnt）のアドレスピン切り替え
    if (g_CS_cnt & 0x01)  A_HIGH; else A_LOW;
    if (g_CS_cnt & 0x02)  B_HIGH; else B_LOW;
    if (g_CS_cnt & 0x04)  C_HIGH; else C_LOW;
    if (g_CS_cnt & 0x08)  D_HIGH; else D_LOW;
    if (g_CS_cnt & 0x10)  E_HIGH; else E_LOW;

    // ラッチパルスを送って、シフトレジスタのデータを保持レジスタ（出力バッファ）に転送する
    STB_HIGH;
    busy_wait_us(MTRX_STB_PULSE_WIDTH_US);
    STB_LOW;

    // 今回の行の点灯を開始し、開始時間を記録
    OE_LOW;
    uint64_t start_time = time_us_64();

    // 表示を行っている間に、次の行（next_CS_cnt）のデータを準備してPIOへ転送する
    int next_CS_cnt = g_CS_cnt + 1;
    uint8_t next_pwm_step = pwm_step;
    if (next_CS_cnt >= Matrix_ROWS_SHOW) {
        next_CS_cnt = 0;
        next_pwm_step = (pwm_step + 1) & 0x0F;
    }

    // 高速化のためのローカルキャッシュ
    ST_COLOR_RGB888 (*display_rgb)[Matrix_COLS] = g_display_rgb;
    int row_top = next_CS_cnt;
    int row_bottom = next_CS_cnt + Matrix_ROWS_SHOW;
    ST_COLOR_RGB888 *top_row = display_rgb[row_top];
    ST_COLOR_RGB888 *bottom_row = display_rgb[row_bottom];

    // シフト演算を排除するための比較値 (pixel > pwm_val)
    uint8_t pwm_val = (next_pwm_step << 4) | 0x0F;

    bool r_enabled = !g_disable_red;
    bool g_enabled = !g_disable_green;

    for (int i = 0; i < (Matrix_COLS_BYTE); i++) {
        uint32_t word1 = 0;
        int col_base = MTRX_PIXELS_PER_ITER * i;
        for (int j = 0; j < MTRX_PIXELS_PER_WORD; j++) {
            int col = col_base + j;
            uint8_t top_r = top_row[col].r;
            uint8_t top_g = top_row[col].g;
            uint8_t top_b = top_row[col].b;
            uint8_t bottom_r = bottom_row[col].r;
            uint8_t bottom_g = bottom_row[col].g;
            uint8_t bottom_b = bottom_row[col].b;

            uint8_t d = 0;
            if (r_enabled && (top_r > pwm_val))   d |= (1 << MTRX_BIT_R1);
            if (g_enabled && (top_g > pwm_val))   d |= (1 << MTRX_BIT_G1);
            if (top_b > pwm_val)                  d |= (1 << MTRX_BIT_B1);

            if (r_enabled && (bottom_r > pwm_val))   d |= (1 << MTRX_BIT_R2);
            if (g_enabled && (bottom_g > pwm_val))   d |= (1 << MTRX_BIT_G2);
            if (bottom_b > pwm_val)                  d |= (1 << MTRX_BIT_B2);

            word1 |= ((uint32_t)d) << (MTRX_PIXEL_DATA_WIDTH * j);
        }
        pio_sm_put_blocking(g_pio, g_sm, word1);

        // 途中で点灯時間をチェックして、規定の点灯時間に達していたら消灯する
        if ((time_us_64() - start_time) >= MTRX_ACTIVE_DURATION_US) {
            OE_HIGH;
        }

        uint32_t word2 = 0;
        for (int j = MTRX_PIXELS_PER_WORD; j < MTRX_PIXELS_PER_ITER; j++) {
            int col = col_base + j;
            uint8_t top_r = top_row[col].r;
            uint8_t top_g = top_row[col].g;
            uint8_t top_b = top_row[col].b;
            uint8_t bottom_r = bottom_row[col].r;
            uint8_t bottom_g = bottom_row[col].g;
            uint8_t bottom_b = bottom_row[col].b;

            uint8_t d = 0;
            if (r_enabled && (top_r > pwm_val))   d |= (1 << MTRX_BIT_R1);
            if (g_enabled && (top_g > pwm_val))   d |= (1 << MTRX_BIT_G1);
            if (top_b > pwm_val)                  d |= (1 << MTRX_BIT_B1);

            if (r_enabled && (bottom_r > pwm_val))   d |= (1 << MTRX_BIT_R2);
            if (g_enabled && (bottom_g > pwm_val))   d |= (1 << MTRX_BIT_G2);
            if (bottom_b > pwm_val)                  d |= (1 << MTRX_BIT_B2);

            word2 |= ((uint32_t)d) << (MTRX_PIXEL_DATA_WIDTH * (j - MTRX_PIXELS_PER_WORD));
        }
        pio_sm_put_blocking(g_pio, g_sm, word2);

        // 途中で点灯時間をチェックして、規定の点灯時間に達していたら消灯する
        if ((time_us_64() - start_time) >= MTRX_ACTIVE_DURATION_US) {
            OE_HIGH;
        }
    }

    // PIOの物理的な送信完了（ストール）をfdebugレジスタで正確に同期
    uint32_t stall_mask = 1u << (PIO_FDEBUG_TXSTALL_LSB + g_sm);
    g_pio->fdebug = stall_mask;
    while (!(g_pio->fdebug & stall_mask)) {
        if ((time_us_64() - start_time) >= MTRX_ACTIVE_DURATION_US) {
            OE_HIGH;
        }
        tight_loop_contents();
    }

    // 規定の点灯時間に達するまで待機（転送が目標時間未満で終わった場合のセーフティ）
    while ((time_us_64() - start_time) < MTRX_ACTIVE_DURATION_US) {
        tight_loop_contents();
    }

    // 最終的に消灯を確認
    OE_HIGH;

    // アドレス・PWM更新
    g_CS_cnt = next_CS_cnt;
    pwm_step = next_pwm_step;
}
