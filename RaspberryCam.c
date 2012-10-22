//============================================================================
// Name        : RaspberryCam.cpp
// Author      : Flechner Romain
// Version     :
// Copyright   : Open source based on fswebcam and v4l source codes
// Description : RaspberryCam in C, Ansi-style
//============================================================================

#ifdef HAVE_CONFIG_H
#include "config.h"
#endif

#include "RaspberryCam.h"

#include <stdio.h>
#include <getopt.h>
#include <string.h>
#include <stdlib.h>
#include <unistd.h>
#include <time.h>
#include <gd.h>
#include <errno.h>
#include <signal.h>
#include <sys/types.h>
#include <sys/stat.h>

#include "log.h"
#include "src.h"
#include "parse.h"

gdImage* fswc_gdImageDuplicate(gdImage* src)
{
	gdImage *dst;
	
	dst = gdImageCreateTrueColor(gdImageSX(src), gdImageSY(src));
	if(!dst) return(NULL);
	
	gdImageCopy(dst, src, 0, 0, 0, 0, gdImageSX(src), gdImageSY(src));
	
	return(dst);
}

void SaveImageToJpegFile(char *filename, gdImagePtr im)
{
  FILE *out;
  int size;
  char *data;
  out = fopen(filename, "wb");
  if (!out) {
    /* Error */
  }
  
  data = (char *) gdImageJpegPtr(im, &size, 100);
  
  if (!data) {
    /* Error */
  }
  if (fwrite(data, 1, size, out) != size) {
    /* Error */
  }
  if (fclose(out) != 0) {
    /* Error */
  }
  gdFree(data);  
}

pictureBuffer ConvertToPictureBuffer(gdImagePtr im, int quantity) {
	pictureBuffer buffer;
	
	memset(&buffer, 0, sizeof(buffer));
	
	buffer.data = (char *) gdImageJpegPtr(im, &(buffer.size), quantity);
	
	return buffer;
}

src_t *OpenCameraStream(char *device, int width, int height, int fps) {
	src_t *src = (src_t*)malloc(sizeof(src_t));
	
	//stdout = fopen("RaspberryCamLogs.txt", "w");
	
	src->input = strdup("0");
	src->tuner = 0;
	src->frequency = 0;
	src->delay = 0;
	src->use_read = 0;
	src->list = 0;
	src->fps = fps;
	src->palette = SRC_PAL_ANY;
	src->option = NULL;
	src->timeout = 10;
	src->width = width;
	src->height = height;
	
	if(src_open(src, device) == -1)
		return NULL;
	
	return src;
}

void CloseCameraStream(src_t *src) {
	src_close(src);
}

int fswc_add_image_rgb565(src_t *src, avgbmp_t *abitmap)
{
	uint16_t *img = (uint16_t *) src->img;
	uint32_t i = src->width * src->height;
	
	if(src->length >> 1 < i) return(-1);
	
	while(i-- > 0)
	{
		uint8_t r, g, b;
		
		r = (*img & 0xF800) >> 8;
		g = (*img &  0x7E0) >> 3;
		b = (*img &   0x1F) << 3;
		
		*(abitmap++) += r + (r >> 5);
		*(abitmap++) += g + (g >> 6);
		*(abitmap++) += b + (b >> 5);
		
		img++;
	}
	
	return(0);
}

pictureBuffer GrabVideoFrame(src_t *src) {
	pictureBuffer buffer;
	
	src_grab(src);
	
	memset(&buffer, 0, sizeof(buffer));
	
	buffer.size = src->length;
	buffer.data = (char *) src->img;
	
	return buffer;
}

pictureBuffer ReadVideoFrame1(src_t *src, int jpegQuantity) {
	avgbmp_t *abitmap, *pbitmap;
	gdImage *image, *original;
	uint32_t frame;
	uint32_t x, y;
	uint8_t modified;
	gdImage *im;
	int frames = 1;
	pictureBuffer buffer;
	
	//stdout = fopen("RaspberryCamLogs.txt", "w");
	
	abitmap = (avgbmp_t*)calloc(src->width * src->height * 3, sizeof(avgbmp_t));
	if(!abitmap)
	{
		puts("Out of memory.");
		return buffer;
	}
	
	src_grab(src);
	
	fswc_add_image_jpeg(src, abitmap);
	
	original = gdImageCreateTrueColor(src->width, src->height);
	if(!original)
	{
		puts("Out of memory.");
		free(abitmap);
		return buffer;
	}
	
	pbitmap = abitmap;
		
	for(y = 0; y < src->height; y++)
		for(x = 0; x < src->width; x++)
		{
			int px = x;
			int py = y;
			int colour;
			
			colour  = (*(pbitmap++) / frames) << 16;
			colour += (*(pbitmap++) / frames) << 8;
			colour += (*(pbitmap++) / frames);
			
			gdImageSetPixel(original, px, py, colour);
		}
	
	free(abitmap);
	
	image = fswc_gdImageDuplicate(original);
	if(!image)
	{
		puts("Out of memory.");
		gdImageDestroy(image);
		return buffer;
	}
	
	gdImageDestroy(original);
	
	memset(&buffer, 0, sizeof(buffer));
	
	if (image == NULL) {
		puts("image is NULL");
		return buffer;
	}
	
	buffer.data = (char *) gdImageJpegPtr(image, &buffer.size, jpegQuantity);
	
	gdImageDestroy(image);
	
	return buffer;
}

pictureBuffer TakePicture(char *device, int width, int height, int jpegQuantity) {
	pictureBuffer buffer;
	
	memset(&buffer, 0, sizeof(buffer));
	
	gdImage *image = grabPicture(strdup(device), width, height);
	
	if (image == NULL) {
		puts("image is NULL");
		return buffer;
	}
	
	buffer.data = (char *) gdImageJpegPtr(image, &buffer.size, jpegQuantity);
	
	gdImageDestroy(image);
	
	return buffer;
}

gdImage *grabPicture(char *device, int width, int height) {
	avgbmp_t *abitmap, *pbitmap;
	gdImage *image, *original;
	src_t src;
	uint32_t frame;
	uint32_t x, y;
	uint8_t modified;
	gdImage *im;
	int frames = 1;
	
	memset(&src, 0, sizeof(src));
	
	src.input = strdup("0");
	src.tuner = 0;
	src.frequency = 0;
	src.delay = 0;
	src.use_read = 0;
	src.list = 0;
	src.fps = 0;
	src.palette = SRC_PAL_ANY;
	src.option = NULL;
	src.timeout = 10;
	src.width = width;
	src.height = height;
	
	if(src_open(&src, device) == -1)
		return NULL;
	
	abitmap = (avgbmp_t*)calloc(src.width * src.height * 3, sizeof(avgbmp_t));
	if(!abitmap)
	{
		puts("Out of memory.");
		return NULL;
	}
	
	src_grab(&src);
	
	fswc_add_image_jpeg(&src, abitmap);
	
	src_close(&src);
	
	original = gdImageCreateTrueColor(src.width, src.height);
	if(!original)
	{
		puts("Out of memory.");
		free(abitmap);
		return NULL;
	}
	
	pbitmap = abitmap;
	
	for(y = 0; y < src.height; y++)
		for(x = 0; x < src.width; x++)
		{
			int px = x;
			int py = y;
			int colour;
			
			colour  = (*(pbitmap++) / frames) << 16;
			colour += (*(pbitmap++) / frames) << 8;
			colour += (*(pbitmap++) / frames);
			
			gdImageSetPixel(original, px, py, colour);
		}
	
	free(abitmap);
	
	image = fswc_gdImageDuplicate(original);
	if(!image)
	{
		puts("Out of memory.");
		gdImageDestroy(image);
		return NULL;
	}
	
	gdImageDestroy(original);
	
	return image;
}

pictureBuffer ReadVideoFrame(src_t *src, int jpegQuantity) {
	pictureBuffer buffer;
	
	uint32_t x, y, hlength;
	uint8_t *himg = NULL;
	gdImage *im;
	int i;
	
	memset(&buffer, 0, sizeof(buffer));
	
	src_grab(src);
	
//	puts("ReadVideoFrame");
	
	/* MJPEG data may lack the DHT segment required for decoding... */
	i = verify_jpeg_dht(src->img, src->length, &himg, &hlength);
	
	im = gdImageCreateFromJpegPtr(hlength, himg);
	if(i == 1) free(himg);
	
	if(!im)
		return buffer;
	
	buffer = ConvertToPictureBuffer(im, jpegQuantity);
	
	gdImageDestroy(im);
	
	return buffer;
}

int main(void) {
	FILE *out;
	src_t *src = OpenCameraStream("/dev/video0", 640, 480, 20);
	pictureBuffer buffer;
	
	uint32_t x, y, hlength;
	uint8_t *himg = NULL;
	gdImage *im;
	int i;
	
	memset(&buffer, 0, sizeof(buffer));
	
	src_grab(src);
	//buffer.data = (char*) src->img;
	//buffer.size = src->length;
	
	/* MJPEG data may lack the DHT segment required for decoding... */
	i = verify_jpeg_dht(src->img, src->length, &himg, &hlength);
	
	im = gdImageCreateFromJpegPtr(hlength, himg);
	if(i == 1) free(himg);
	
	if(!im)
		return(-1);
	
	
	buffer = ConvertToPictureBuffer(im, 100);
	
	gdImageDestroy(im);
	
	out = fopen("out5.jpg", "wb");
	if (fwrite(buffer.data, 1, buffer.size, out) != buffer.size) {
		puts("Error");
	}
	fclose(out);
	
	CloseCameraStream(src);
	
	free(buffer.data);
	
	return EXIT_SUCCESS;
}

int main1(void) {

	gdImage *image = grabPicture(strdup("/dev/video0"), 640, 480);
	
	if (image == NULL) {
		puts("image is NULL");
		return -1;
	}
	
	SaveImageToJpegFile("out3.jpg",image);
	
	gdImageDestroy(image);
	
	return EXIT_SUCCESS;
}

