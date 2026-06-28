#ifndef __HAL_RGBMATRIX_DEVICE_H
#define __HAL_RGBMATRIX_DEVICE_H

#include "RGBMatrix_device.h"

#ifdef __cplusplus
extern "C" {
#endif

void HAL_RGBMatrixDeviceInit(void);
void HAL_RGBMatrixDeviceFlush(PDisplayDevice ptDev);
void HAL_RGBMatrixDeviceSetRedDisable(bool disable);
void HAL_RGBMatrixDeviceSetGreenDisable(bool disable);

#ifdef __cplusplus
}
#endif

#endif
