#ifndef __RGBMATRIX_DEVICE_H
#define __RGBMATRIX_DEVICE_H

typedef struct DisplayDevice{
	char *name;
	void *FBBase;
	int iXres;
	int iYres;
	void (*Init)(void);
	void (*Flush)(struct DisplayDevice *ptDev);
}DisplayDevice,*PDisplayDevice;

#ifdef __cplusplus
extern "C" {
#endif

PDisplayDevice GetDisplayDevice(void);

#ifdef __cplusplus
}
#endif

#endif
