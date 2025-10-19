#include "Common.h"

static uint8_t g_RGBMatrixFramebuffer[256]={0xff};

static void RGBMatrixDeviceInit()
{
    HAL_RGBMatrixDeviceInit();
}

static  void RGBMatrixDeviceFlush(PDisplayDevice ptDev)
{
    HAL_RGBMatrixDeviceFlush(ptDev);
}

static DisplayDevice g_tRGBMatrixDevice = {
	(char *)"RGBMatrix",
	g_RGBMatrixFramebuffer,
	64,
	32,
    RGBMatrixDeviceInit,
    RGBMatrixDeviceFlush
};

PDisplayDevice GetDisplayDevice(void)
{
    return &g_tRGBMatrixDevice;
}