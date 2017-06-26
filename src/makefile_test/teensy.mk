#******************************************************
# Generated by Board2Make (github\luni\xxx)
#
# Board              Teensy 3.6
# USB Type           Serial
# CPU Speed          180 MHz
# Optimize           Faster
# Keyboard Layout    US English
#
# 26.06.2017 21:56
#******************************************************

BOARD_ID   := TEENSY36

FLAGS_CPU  := -mthumb -mcpu=cortex-m4 -mfloat-abi=hard -mfpu=fpv4-sp-d16 -fsingle-precision-constant
FLAGS_OPT  := -O2
FLAGS_COM  := -g -Wall -ffunction-sections -fdata-sections -nostdlib -MMD
FLAGS_LSP  := 

FLAGS_CPP  := -fno-exceptions -felide-constructors -std=gnu++0x -fno-rtti
FLAGS_C    := 
FLAGS_S    := -x assembler-with-cpp
FLAGS_LD   := -Wl,--gc-sections,--relax,--defsym=__rtc_localtime=0

LIBS       := -larm_cortexM4lf_math -lm
LD_SCRIPT  := mk66fx1m0.ld

DEFINES    := -D__MK66FX1M0__ -DTEENSYDUINO=136
DEFINES    += -DF_CPU=180000000 -DUSB_SERIAL -DLAYOUT_US_ENGLISH

CPP_FLAGS  := $(FLAGS_CPU) $(FLAGS_OPT) $(FLAGS_COM) $(DEFINES) $(FLAGS_CPP)
C_FLAGS    := $(FLAGS_CPU) $(FLAGS_OPT) $(FLAGS_COM) $(DEFINES) $(FLAGS_C)
S_FLAGS    := $(FLAGS_CPU) $(FLAGS_OPT) $(FLAGS_COM) $(DEFINES) $(FLAGS_S)
LD_FLAGS   := $(FLAGS_CPU) $(FLAGS_OPT) $(FLAGS_LSP) $(FLAGS_LD)
AR_FLAGS   := rcs
