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

#include <stdio.h>
#include <getopt.h>
#include <string.h>
#include <stdlib.h>
#include <unistd.h>
#include <time.h>
#include <errno.h>
#include <signal.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <gd.h>
#include <stdint.h>

typedef uint32_t avgbmp_t;

#include "log.h"
#include "src.h"
#include "parse.h"


int main(void) {
	
	//uint32_t frame;
	//uint32_t x, y;
	avgbmp_t *abitmap, *pbitmap;
	//uint8_t modified;
	
	src_t src;
	
	
	/* Record the start time. */
	//config->start = time(NULL);
	
	/* Set source options... */
	memset(&src, 0, sizeof(src));
	/*
	src.input      = config->input;
	src.tuner      = config->tuner;
	src.frequency  = config->frequency;
	src.delay      = config->delay;
	src.timeout    = 10; //seconds
	src.use_read   = config->use_read;
	src.list       = config->list;
	src.palette    = config->palette;
	src.width      = config->width;
	src.height     = config->height;
	src.fps        = config->fps;
	src.option     = config->option;
	*/
	
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
	
	char *dumpframe = "grab.raw";
	
	printf("Dumping raw frame to '%s'...", dumpframe);
	
	f = fopen(dumpframe, "wb");
	if(!f)
		printf("fopen: %s", strerror(errno));
	else
	{
		fwrite(src.img, 1, src.length, f);
		fclose(f);
	}
	
	
	puts("Bye bye ...");
	return EXIT_SUCCESS;
}

