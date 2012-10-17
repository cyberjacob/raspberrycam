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
  //data = (char *) gdImagePngPtr(im, &size);
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

src_t *FakeOpen(char *device, int width, int height) {
	src_t *src = (src_t*)malloc(sizeof(src_t));
	
	//memset(&src, 0, sizeof(src));
	
	src->input = strdup("0");
	src->tuner = 0;
	src->frequency = 0;
	src->delay = 0;
	src->use_read = 0;
	src->list = 0;
	src->fps = 0;
	src->palette = SRC_PAL_ANY;
	src->option = NULL;
	src->timeout = 10;
	src->width = width;
	src->height = height;
	
	return src;
}

src_t *OpenCameraStream(char *device, int width, int height, int fps) {
	src_t *src = (src_t*)malloc(sizeof(src_t));
	
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

pictureBuffer ReadVideoFrame(src_t *src, int jpegQuantity) {
	avgbmp_t *abitmap, *pbitmap;
	gdImage *image, *original;
	uint32_t frame;
	uint32_t x, y;
	uint8_t modified;
	gdImage *im;
	int frames = 1;
	pictureBuffer buffer;
	
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

void DisplaySrc(src_t *src) {
	puts("DisplaySrc 2");
	
	printf("input: %s\n", src->input);
	
	printf("width: %d\n", src->width);
	printf("height: %d\n", src->height);
}

pictureBuffer TakePicture(char *device, int width, int height, int jpegQuantity) {
	pictureBuffer buffer;
	
	//freopen("/dev/null", "w", stdout);
	
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
		return(-1);
	}
	
	gdImageDestroy(original);
	
	return image;
}



int main(void) {

	gdImage *image = grabPicture(strdup("/dev/video0"), 640, 480);
	
	if (image == NULL) {
		puts("image is NULL");
		return -1;
	}
	
	SaveImageToJpegFile("out3.jpg",image);
	
	gdImageDestroy(image);
	
	/*
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
	
	//src.width = 384;
	//src.height = 288;
	
	
	src.width = 640;
	src.height = 480;
	
	char *device = strdup("/dev/video0");
	
	printf("--- Opening %s...", device);
	
	if(src_open(&src, device) == -1) return(-1);
	
	abitmap = (avgbmp_t*)calloc(src.width * src.height * 3, sizeof(avgbmp_t));
	if(!abitmap)
	{
		puts("Out of memory.");
		return(-1);
	}
	
	src_grab(&src);
	
	FILE *f;
	
	fswc_add_image_jpeg(&src, abitmap);
	
	src_close(&src);
	
	original = gdImageCreateTrueColor(src.width, src.height);
	if(!original)
	{
		puts("Out of memory.");
		free(abitmap);
		return(-1);
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
		return(-1);
	}
	
	im = fswc_gdImageDuplicate(image);
	if(!im)
	{
		puts("Out of memory.");
		return(-1);
	}
	
	char *filename = "out.jpg";
	
	f = fopen("out.jpg", "wb");
	if(!f)
	{
		printf("Error opening file for output: %s", filename);
		printf("fopen: %s", strerror(errno));
		gdImageDestroy(im);
		return(-1);
	}
	
	printf("Writing JPEG image to '%s'.", filename);
	gdImageJpeg(im, f, 90);
	
	SaveImageToJpegFile("out2.jpg", im);
	
	if(f != stdout) fclose(f);
	
	gdImageDestroy(im);
	
	*/
	
	return EXIT_SUCCESS;
}

