#ifndef RASPBERRYCAM_H_
#define RASPBERRYCAM_H_

#include <stdint.h>

#ifdef HAVE_CONFIG_H
#include "config.h"
#endif

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


#endif /*RASPBERRYCAM_H_*/
