#************************************************************************
# TARGET: filename of the generated library (.a)
#************************************************************************
TARGET = $(CORE_LIB)

#************************************************************************
# PATHS 
#************************************************************************
SRC	:= src
BIN := obj
L_INC := -I./$(SRC) 

#************************************************************************
# Source Files
#************************************************************************
#
# Recursively create list of source and object files in subdirectories of
# SRC (taken from http://stackoverflow.com/a/12959694)

rwildcard=$(wildcard $1$2) $(foreach d,$(wildcard $1*),$(call rwildcard,$d/,$2))

CPP_FILES	:= $(call rwildcard,$(SRC)/,*.cpp)
C_FILES		:= $(call rwildcard,$(SRC)/,*.c)
S_FILES		:= $(call rwildcard,$(SRC)/,*.S)
OBJECTS		:= $(S_FILES:.S=.o) $(C_FILES:.c=.o) $(CPP_FILES:.cpp=.o)
OBJ			:= $(foreach src,$(OBJECTS), $(BIN)/$(src))

#************************************************************************
# Rules:
#************************************************************************

.PHONY: all clean

all: $(TARGET)
	@:

clean:
	@echo cleaning core binaries...
	@if exist $(BIN) rd /s/q "$(BIN)"	
	@if exist $(TARGET) del  $(TARGET)

$(TARGET) : $(OBJ)
	@echo [AR]	$@
	@$(AR) rcs $@ $^	
	@echo teensy core library built successfully &&echo.
	
$(BIN)/%.o: %.S
	@echo [ASM]	$@
	@if not exist $(dir $@) @mkdir "$(dir $@)"
	@$(CC) $(S_FLAGS) $(L_INC) -o $@ -c $<

$(BIN)/%.o: %.c
	@echo [CC]	$<
	@if not exist $(dir $@) @mkdir "$(dir $@)"
	@$(CC) $(C_FLAGS) $(L_INC) -o $@ -c $< 

$(BIN)/%.o: %.cpp
	@echo [CXX]	$<
	@if not exist $(dir $@) @mkdir "$(dir $@)"
	@$(CXX) $(CPP_FLAGS) $(L_INC) -o $@ -c $< 

