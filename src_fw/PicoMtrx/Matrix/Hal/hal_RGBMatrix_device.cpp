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

