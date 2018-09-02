using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Board2Make.Model
{
    class Model
    {
        public string boardTxtPath { get; set; }
        public string compilerPath { get; set; }
        public string corePath { get; set; }
        public string projectPath { get; set; }
        public string makePath { get; set; }
        public string uploadPathTeensy { get; set; }
        public string uploadPathTY { get; set; }

        public List<Board> boards { get; private set; }

        public void parseBoardsTxt(string boards_txt)
        {
            if (File.Exists(boards_txt))
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey("Software\\lunOptics\\Board2Make");
                key.SetValue("input", boards_txt);

                var content = new FileContent(boards_txt);
                if (!content.ParseError)
                {
                    boards = content.boards.Where(b => b.core.ToUpper() == "TEENSY3").ToList();
                }
            }
        }

        public string generateTasksFile()
        {
            if (makePath == null) return "Please set the path to make.exe";

            var tasks = new tasksJson()
            {
                presentation = new Presentation(),
                problemMatcher = new ProblemMatcher(),
                tasks = new List<Task>()
                {
                    new Task()
                    {
                        label = "Build all",
                        group = new Group(),
                        command = makePath.Replace('\\','/'),
                        args = new List<string>{"all"},
                    },
                    new Task()
                    {
                        label = "Rebuild User Code",
                        group = new Group(),
                        command = makePath.Replace('\\','/'),
                        args = new List<string>{"rebuild"},
                    },
                    new Task()
                    {
                        label = "Clean",
                        group = new Group(),
                        command = makePath.Replace('\\','/'),
                        args = new List<string>{"clean"},
                    },
                    new Task()
                    {
                        label = "Upload (Teensy Uploader)",
                        group = new Group(),
                        command = makePath.Replace('\\','/'),
                        args = new List<string>{"upload"},
                    }
                    ,
                    new Task()
                    {
                        label = "Upload (TyCommander)",
                        group = new Group(),
                        command = makePath.Replace('\\','/'),
                        args = new List<string>{"uploadTy"},
                    }
                }
            };
            var json = new JavaScriptSerializer();
            return FileHelpers.FormatOutput(json.Serialize(tasks));
        }

        public string generatePropertiesFile()
        {
            if (compilerPath == null) return "Please set a compiler path";
            if (corePath == null) return "Please set a the path to the core library";

            var props = new PropertiesJson()
            {
                configurations = new List<Configuration>()
                {
                    new Configuration()
                    {
                        name = "Teensy",
                        compilerPath =  Path.Combine(compilerPath ,"bin","arm-none-eabi-gcc.exe").Replace('\\','/'),
                        intelliSenseMode = "gcc-x64",
                        includePath = new List<string>()
                        {
                            "${workspaceFolder}/src/**",
                            corePath?.Replace('\\','/') + "/**"
                        },
                        defines = new List<string>()
                        {
                            "USB_SERIAL",
                            "UNICODE"
                        }

                    }
                }
            };
            var json = new JavaScriptSerializer();
            return FileHelpers.FormatOutput(json.Serialize(props));
        }

        public string generateMakefile(Board board /* ,IEnumerable<Option> options*/)
        {
            if (board == null) return "ERROR";

            if (boardTxtPath == null) return "Path to board.txt is missing";
            if (compilerPath == null) return "Path to compiler is missing";
            if (corePath == null) return "Path to core library is missing";

            var ao = board.getAllOptions();

            StringBuilder mf = new StringBuilder();

            mf.Append("#******************************************************************************\n");
            mf.Append("# Generated by Board2Make (https://github.com/luni64/Board2Make)\n");
            mf.Append("#\n");
            mf.Append($"# {"Board",-18} {board.name}\n");
            board.optionSets.ForEach(o => mf.Append($"# {o.name,-18} {o.selectedOption.name}\n"));
            mf.Append("#\n");
            mf.Append($"# {"Boards.txt",-18} {boardTxtPath}\n");

            mf.Append("#\n");
            mf.Append($"# {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}\n");
            mf.Append("#******************************************************************************\n\n");

            mf.Append("TARGET_NAME := newtest\n");

            mf.Append($"GCC_BASE    := {compilerPath}\n");
            mf.Append($"CORE_BASE   := {corePath}\n");
            mf.Append($"UPL_PJRC_B  := {uploadPathTeensy}\n");
            mf.Append($"UPL_TYCMD_B := {uploadPathTY}\n");

            mf.Append($"USR_SRC     := src\n");

            mf.Append(makeEntry("BOARD_ID    := ", "build.board", ao) + "\n");
            mf.Append($"\n");

            mf.Append(makeEntry("FLAGS_CPU   := ", "build.flags.cpu", ao) + "\n");
            mf.Append(makeEntry("FLAGS_OPT   := ", "build.flags.optimize", ao) + "\n");
            mf.Append(makeEntry("FLAGS_COM   := ", "build.flags.common", ao) + makeEntry(" ", "build.flags.dep", ao) + "\n");
            mf.Append(makeEntry("FLAGS_LSP   := ", "build.flags.ldspecs", ao) + "\n");

            mf.Append("\n");
            mf.Append(makeEntry("FLAGS_CPP   := ", "build.flags.cpp", ao) + "\n");
            mf.Append(makeEntry("FLAGS_C     := ", "build.flags.c", ao) + "\n");
            mf.Append(makeEntry("FLAGS_S     := ", "build.flags.S", ao) + "\n");
            mf.Append(makeEntry("FLAGS_LD    := ", "build.flags.ld", ao) + "\n");

            mf.Append("\n");
            mf.Append(makeEntry("LIBS        := ", "build.flags.libs", ao) + "\n");
            mf.Append(makeEntry("LD_SCRIPT   := ", "build.mcu", ao) + ".ld\n");

            mf.Append("\n");
            mf.Append(makeEntry("DEFINES     := ", "build.flags.defs", ao) + "\n");
            mf.Append("DEFINES     += ");
            mf.Append(makeEntry("-DF_CPU=", "build.fcpu", ao) + " " + makeEntry("-D", "build.usbtype", ao) + " " + makeEntry("-DLAYOUT_", "build.keylayout", ao) + "\n");

            mf.Append($"\n");
            mf.Append("CPP_FLAGS   := $(FLAGS_CPU) $(FLAGS_OPT) $(FLAGS_COM) $(DEFINES) $(FLAGS_CPP)\n");
            mf.Append("C_FLAGS     := $(FLAGS_CPU) $(FLAGS_OPT) $(FLAGS_COM) $(DEFINES) $(FLAGS_C)\n");
            mf.Append("S_FLAGS     := $(FLAGS_CPU) $(FLAGS_OPT) $(FLAGS_COM) $(DEFINES) $(FLAGS_S)\n");
            mf.Append("LD_FLAGS    := $(FLAGS_CPU) $(FLAGS_OPT) $(FLAGS_LSP) $(FLAGS_LD)\n");
            mf.Append("AR_FLAGS    := rcs\n");
            mf.Append(mkf);

            return mf.ToString();
        }

        string makeEntry(String txt, String key, Dictionary<String, String> options)
        {
            if (options.ContainsKey(key))
            {
                return $"{txt}{options[key]}";
            }
            else return "";
        }


        public Model(string boards_txt)
        {
            parseBoardsTxt(boards_txt);
        }

        const string mkf =
"\n" +
"USR_SRC     := src\n" +
"CORE_SRC    := $(CORE_BASE)\n" +
"\n" +
"BIN         := bin\n" +
"USR_BIN     := $(BIN)\\src\n" +
"CORE_BIN    := $(BIN)\\core\n" +
"CORE_LIB    := $(BIN)\\core.a\n" +
"TARGET_HEX  := $(BIN)\\$(TARGET_NAME).hex\n" +
"TARGET_ELF  := $(BIN)\\$(TARGET_NAME).elf\n" +
"TARGET_LST  := $(BIN)\\$(TARGET_NAME).lst\n" +
"\n\n" +
"#******************************************************************************\n" +
"# BINARIES\n" +
"#******************************************************************************\n" +
"CC          := $(GCC_BASE)/bin/arm-none-eabi-gcc\n" +
"CXX         := $(GCC_BASE)\\bin\\arm-none-eabi-g++\n" +
"AR          := $(GCC_BASE)\\bin\\arm-none-eabi-gcc-ar\n" +
"OBJCOPY     := $(GCC_BASE)\\bin\\arm-none-eabi-objcopy\n" +
"SIZE        := $(GCC_BASE)\\bin\\arm-none-eabi-size\n" +
"OBJDUMP     := $(GCC_BASE)\\bin\\arm-none-eabi-objdump\n" +
"UPL_PJRC    := \"$(UPL_PJRC_B)\\teensy_post_compile\" -test -file=$(TARGET_NAME) -path=$(BIN) -tools=\"$(UPL_PJRC_B)\" -board=$(BOARD_ID) -reboot\n" +
"UPL_TYCMD   := $(UPL_TYCMD_B)\\tyCommanderC upload $(TARGET_HEX) --autostart\n" +
"\n" +
"#******************************************************************************\n" +
"# Source and Include Files\n" +
"#******************************************************************************\n" +
"# Recursively create list of source and object files in USR_SRC and CORE_SRC \n" +
"# and corresponding subdirectories. \n" +
"# The function rwildcard is taken from http://stackoverflow.com/a/12959694)\n" +
"\n" +
"rwildcard =$(wildcard $1$2) $(foreach d,$(wildcard $1*),$(call rwildcard,$d/,$2))\n" +
"\n" +
"#User Sources -----------------------------------------------------------------\n" +
"USR_C_FILES    := $(call rwildcard,$(USR_SRC)/,*.c)\n" +
"USR_CPP_FILES  := $(call rwildcard,$(USR_SRC)/,*.cpp)\n" +
"USR_S_FILES    := $(call rwildcard,$(USR_SRC)/,*.S)\n" +
"USR_OBJ        := $(USR_S_FILES:$(USR_SRC)/%.S=$(USR_BIN)/%.o) $(USR_C_FILES:$(USR_SRC)/%.c=$(USR_BIN)/%.o) $(USR_CPP_FILES:$(USR_SRC)/%.cpp=$(USR_BIN)/%.o) \n" +
"\n" +
"# Core library sources -------------------------------------------------------- \n" +
"CORE_CPP_FILES := $(call rwildcard,$(CORE_SRC)/,*.cpp)\n" +
"CORE_C_FILES   := $(call rwildcard,$(CORE_SRC)/,*.c)\n" +
"CORE_OBJ       := $(CORE_S_FILES:$(CORE_SRC)/%.S=$(CORE_BIN)/%.o) $(CORE_C_FILES:$(CORE_SRC)/%.c=$(CORE_BIN)/%.o) $(CORE_CPP_FILES:$(CORE_SRC)/%.cpp=$(CORE_BIN)/%.o) \n" +
"\n" +
"#$(info INFO: ${USR_OBJ})\n" +
"#$(info INFO: ${CORE_OBJ})\n" +
"\n" +
"INCLUDE        := -I.\\$(USR_SRC) -I$(CORE_SRC)\n" +
"\n" +
"\n" +
"#******************************************************************************\n" +
"# Rules:\n" +
"#******************************************************************************\n" +
"\n" +
".PHONY: all rebuild upload uploadTy clean cleanUser cleanCore\n" +
"\n" +
"all:     $(TARGET_LST) $(TARGET_HEX)\n" +
"rebuild: cleanUser all\n" +
"clean:   cleanUser cleanCore\n" +
"\n" +
"upload: $(TARGET_LST) $(TARGET_HEX)\n" +
"	@$(UPL_PJRC)\n" +
"\n" +
 "uploadTy: $(TARGET_LST) $(TARGET_HEX)\n" +
"	@$(UPL_TYCMD)\n" +
"\n" +
"# Core library ----------------------------------------------------------------\n" +
"$(CORE_BIN)/%.o: $(CORE_SRC)/%.S\n" +
"\t@echo [ASM] CORE $(notdir $<)\n" +
"\t@if not exist $(dir $@)  @mkdir \"$(dir $@)\"\n" +
"\t@\"$(CC)\" $(S_FLAGS) $(INCLUDE) -o $@ -c $< \n" +
"\n" +
"$(CORE_BIN)/%.o: $(CORE_SRC)/%.c\n" +
"\t@echo [CC]  CORE $(notdir $<)\n" +
"\t@if not exist $(dir $@)  @mkdir \"$(dir $@)\"\n" +
"\t@\"$(CC)\" $(C_FLAGS) $(INCLUDE) -o $@ -c $< \n" +
"\n" +
"$(CORE_BIN)/%.o: $(CORE_SRC)/%.cpp\n" +
"\t@echo [CPP] CORE $(notdir $<)\n" +
"\t@if not exist $(dir $@)  @mkdir \"$(dir $@)\"\n" +
"\t@\"$(CXX)\" $(CPP_FLAGS) $(INCLUDE) -o $@ -c $< \n" +
"\n" +
"$(CORE_LIB) : $(CORE_OBJ)\n" +
"\t@echo [AR]  $@\n" +
"\t@$(AR) $(AR_FLAGS) $@ $^\n" +
"\t@echo Teensy core built successfully &&echo.\n" +
"\n" +
"# Handle user sources ---------------------------------------------------------\n" +
"$(USR_BIN)/%.o: $(USR_SRC)/%.S\n" +
"\t@echo [ASM] $<\n" +
"\t@if not exist $(dir $@)  @mkdir \"$(dir $@)\"\n" +
"\t@\"$(CC)\" $(S_FLAGS) $(INCLUDE) -o $@ -c $<\n" +
"\n" +
"$(USR_BIN)/%.o: $(USR_SRC)/%.c\n" +
"\t@echo [CC]  $(notdir $<)\n" +
"\t@if not exist $(dir $@)  @mkdir \"$(dir $@)\"\n" +
"\t@\"$(CC)\" $(C_FLAGS) $(INCLUDE) -o \"$@\" -c $<\n" +
"\n" +
"$(USR_BIN)/%.o: $(USR_SRC)/%.cpp\n" +
"\t@echo [CPP] $<\n" +
"\t@if not exist $(dir $@)  @mkdir \"$(dir $@)\"\n" +
"\t@\"$(CXX)\" $(CPP_FLAGS) $(INCLUDE) -o \"$@\" -c $<\n" +
"\n" +
"# Linking ---------------------------------------------------------------------\n" +
"$(TARGET_ELF): $(CORE_LIB) $(USR_OBJ)\n" +
"\t@echo [LD]  $@\n" +
"\t@$(CC) $(LD_FLAGS) -T$(CORE_SRC)/$(LD_SCRIPT) -o \"$@\" $(USR_OBJ) $(CORE_LIB) $(LIBS)\n" +
"\t@echo User code built and linked to core lib &&echo.\n" +
"\n" +
"%.lst: %.elf\n" +
"\t@echo [LST] $@\n" +
"\t@$(OBJDUMP) -d -S --demangle --no-show-raw-insn --syms \"$<\"  > \"$@\"\n" +
"\t@echo Listfile generated &&echo.\n" +
"\n" +
"%.hex: %.elf\n" +
"\t@echo [HEX] $@\n" +
"\t@$(SIZE) \"$<\"\n" +
"\t@$(OBJCOPY) -O ihex -R.eeprom \"$<\" \"$@\"\n" +
"\t@echo Sucessfully built project &&echo.\n" +
"\n" +
"# Cleaning --------------------------------------------------------------------\n" +
"cleanUser:\n" +
"\t@echo Cleaning user binaries...\n" +
"\t@if exist $(USR_BIN) rd /s/q \"$(USR_BIN)\"\n" +
"\n" +
"\t@if exist $(TARGET_HEX) del  $(TARGET_HEX)\n" +
"\t@if exist $(TARGET_ELF) del  $(TARGET_ELF)\n" +
"\t@if exist $(TARGET_LST) del  $(TARGET_LST)\n" +
"\n" +
"cleanCore:\n" +
"\t@echo Cleaning core binaries...\n" +
"\t@if exist $(CORE_BIN) rd /s/q \"$(CORE_BIN)\"\n" +
"\t@if exist $(CORE_LIB) del  \"$(CORE_LIB)\"\n" +
"\n" +
"# compiler generated dependency info ------------------------------------------\n" +
"-include $(CORE_OBJ:.o=.d)\n" +
"-include $(USR_OBJ:.o=.d)\n";


    }
}
