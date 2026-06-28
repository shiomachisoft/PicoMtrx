#include "Common.h"

static uint8_t f_RGBMatrixFramebuffer[Matrix_ROWS * Matrix_COLS_BYTE]={0xff};

static void RGBMatrixDeviceInit(void)
{
    HAL_RGBMatrixDeviceInit();
}

static void RGBMatrixDeviceFlush(PDisplayDevice ptDev)
{
    HAL_RGBMatrixDeviceFlush(ptDev);
}

static DisplayDevice f_tRGBMatrixDevice = {
    (char *)"RGBMatrix",
    f_RGBMatrixFramebuffer,
    Matrix_COLS,
    Matrix_ROWS,
    RGBMatrixDeviceInit,
    RGBMatrixDeviceFlush
};

PDisplayDevice GetDisplayDevice(void)
{
    return &f_tRGBMatrixDevice;
}