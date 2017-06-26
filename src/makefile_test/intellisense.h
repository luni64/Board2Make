
//#define T3_6
//#define T3_5
#define T3_2

#if defined  T3_6
#define __MK66FX1M0__
#define F_CPU 240000000
#elif defined T3_5
#define __MK66FX512__
#define F_CPU 96000000
#elif defined T3_2
#define __MK20DX256__
#define F_CPU 96000000
#endif 

#define TEENSYDUINO = 134
#define ARDUINO = 10613
#define USB_SERIAL
#define LAYOUT_US_ENGLISH
//
#if defined(_MSC_VER) && (_MSC_VER < 1800 || defined(__cplusplus))
#undef __cplusplus
#define __cplusplus 201103L
#endif

#define _HAVE_STDC

#define __attribute__(A) /* do nothing */
#define __extension__  /* do nothing */

//#include "src\HAL\OCEAN_EMBED_2000.h"

#define volatile
#define __asm__(x)

//#include <Arduino.h>
//#include <algorithm>

