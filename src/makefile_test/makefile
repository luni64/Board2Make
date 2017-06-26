#************************************************************************
# TARGET: filename of the generated output (.hex, .elf, .list)
#************************************************************************
TARGET = makeTest

#************************************************************************
# PATHS and FILES
#************************************************************************
SRC			:= src
BIN			:= obj
CORE_BASE	:= cores/core_134
CORE_SRC	:= $(CORE_BASE)/src
CORE_LIB	:= core.a

TOOLCHAIN	:= c:/toolchain
GCC_BIN		:= $(TOOLCHAIN)/gcc/gcc-arm-none-eabi-4_8-2014q3-20140805-win32/bin
TEENSY_BIN	:= $(TOOLCHAIN)/teensy

INCLUDE		:= -I./$(SRC) -I./$(CORE_SRC)

#************************************************************************
# BINARIES
#************************************************************************
CC			:= $(GCC_BIN)/arm-none-eabi-gcc
CXX			:= $(GCC_BIN)/arm-none-eabi-g++
AR			:= $(GCC_BIN)/arm-none-eabi-ar
OBJCOPY		:= $(GCC_BIN)/arm-none-eabi-objcopy
SIZE		:= $(GCC_BIN)/arm-none-eabi-size
OBJDUMP		:= $(GCC_BIN)/arm-none-eabi-objdump
SED_EXE		:= $(TOOLCHAIN)/sed.exe

# Reformat gcc error messages for correct parsing by VS
SED_SCRIPT	:= "s,/,\\,g;
SED_SCRIPT	+= s/:\([0-9][0-9]*\):\([0-9][0-9]*\):/(\1,\2): /g"
SED			:= "$(SED_EXE)" -e $(SED_SCRIPT)

#************************************************************************
# Load Compiler and Linker Flags
#************************************************************************

include teensy.mk

export CC  CXX  AR  CORE_LIB  CPP_FLAGS  C_FLAGS  S_FLAGS  AR_FLAGS   #export variables for sub makefiles

#************************************************************************
# Source Files
#************************************************************************
# Recursively create list of source and object files in subdirectories of
# $(SRC) (this is taken from http://stackoverflow.com/a/12959694)

rwildcard=$(wildcard $1$2) $(foreach d,$(wildcard $1*),$(call rwildcard,$d/,$2))

C_FILES		:= $(call rwildcard,$(SRC)/,*.c)
CPP_FILES	:= $(call rwildcard,$(SRC)/,*.cpp)
S_FILES		:= $(call rwildcard,$(SRC)/,*.S)

OBJECTS		:= $(S_FILES:.s=.o) $(C_FILES:.c=.o) $(CPP_FILES:.cpp=.o)
OBJ			:= $(foreach src,$(OBJECTS), $(BIN)/$(src))


#$(info $$OBJ is [${OBJ}])


#******************************************************************************
# Rules:
#******************************************************************************

.PHONY: build rebuild upload core clean cleanUser cleanCore 

build: core $(TARGET).hex

rebuild: cleanUser build

clean: cleanUser cleanCore

upload: $(TARGET).hex
	$(TEENSY_BIN)/teensy_post_compile -test -file="$(basename $<)" -path=$(CURDIR) -tools=$(TEENSY_BIN) -board=$(BOARD_ID) -reboot

# Build Core Libarary --

core:
	@$(MAKE) --directory=$(CORE_BASE) --no-print-directory
	
# Build User Code --

$(BIN)/%.o: %.S
	@echo [ASM]	$<
	@if not exist $(dir $@)  @mkdir "$(dir $@)"
	@"$(CC)" $(S_FLAGS) $(INCLUDE) -o $@ -c $<  2>&1 | $(SED)

$(BIN)/%.o: %.c
	@echo [CC]	$<
	@if not exist $(dir $@)  @mkdir "$(dir $@)"
	@"$(CC)" $(C_FLAGS) $(INCLUDE) -o "$@" -c $< 2>&1 | $(SED)

$(BIN)/%.o: %.cpp
	@echo [CXX]	$<	
	@if not exist $(dir $@)  @mkdir "$(dir $@)"
	@"$(CXX)" $(CPP_FLAGS) $(INCLUDE) -o "$@" -c $< 2>&1 | $(SED)


# Linking --

$(TARGET).elf: $(OBJ) 	
	@echo [LD]	$@
	@$(CC) $(LD_FLAGS) -T$(CORE_SRC)/$(LD_SCRIPT) -o "$@" $(OBJ) $(LIBS) $(CORE_BASE)/$(CORE_LIB)
	@$(OBJDUMP) -d --demangle --no-show-raw-insn --syms $@  > "$(@:.elf=.lst)"
	@echo user code built successfully &&echo.

%.hex: %.elf
	@echo [HEX]	$<
	@$(SIZE) "$<"
	@$(OBJCOPY) -O ihex -R .eeprom "$<" "$@"

# compiler generated dependency info
 -include $(OBJ:.o=.d)

# Cleaning ----------

cleanUser:
	@echo cleaning user binaries...
	@if exist $(BIN) rd /s/q "$(BIN)"
	@if exist $(TARGET).elf del  $(TARGET).elf
	@if exist $(TARGET).hex del  $(TARGET).hex
	@if exist $(TARGET).lst del  $(TARGET).lst

cleanCore:
	@$(MAKE) --directory=$(CORE_BASE) --file=makefile --no-print-directory clean 