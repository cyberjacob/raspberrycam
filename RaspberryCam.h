#ifndef RASPBERRYCAM_H_
#define RASPBERRYCAM_H_

#ifdef HAVE_CONFIG_H
#include "config.h"
#endif

#include <stdint.h>
#include <gd.h>

#include "src.h"

/* Define the bitmap type */
#ifdef USE_32BIT_BUFFER

typedef uint32_t avgbmp_t;
#define MAX_FRAMES (UINT32_MAX >> 8)

#else

typedef uint16_t avgbmp_t;
#define MAX_FRAMES (UINT16_MAX >> 8)

#endif
/*----*/

#define CLIP(val, min, max) (((val) > (max)) ? (max) : (((val) < (min)) ? (min) : (val)))

typedef struct PictureBuffer {
	int size;
	char *data;
} pictureBuffer;

typedef struct VideoStreamHandle {
	
} videoStreamHandle;

gdImage* fswc_gdImageDuplicate(gdImage* src);
void SaveImageToJpegFile(char *filename, gdImagePtr im);
gdImage *grabPicture(char *device, int width, int height);

/* to test .net IntPtr */
extern void DisplaySrc(src_t *src);
extern src_t *FakeOpen(char *device, int width, int height);

/* to take a simple picture */
extern pictureBuffer TakePicture(char *device, int width, int height, int jpegQuantity);

/* to stream a video */
extern src_t *OpenCameraStream(char *device, int width, int height, int fps);
extern void CloseCameraStream(src_t *src);
extern pictureBuffer ReadVideoFrame(src_t *src, int jpegQuantity);

#endif /*RASPBERRYCAM_H_*/
