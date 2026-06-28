#include "Common.h"

void HAL_RGBMatrixDeviceInit(void)
{
    picoRGBMatrixDeviceInit();
}

void HAL_RGBMatrixDeviceFlush(PDisplayDevice ptDev)
{
    uint8_t *buf = (uint8_t *)ptDev->FBBase;
    RGBMatrixDeviceFlush(buf);
}

extern "C" bool g_disable_red;
void HAL_RGBMatrixDeviceSetRedDisable(bool disable)
{
    g_disable_red = disable;
}

extern "C" bool g_disable_green;
void HAL_RGBMatrixDeviceSetGreenDisable(bool disable)
{
    g_disable_green = disable;
}
