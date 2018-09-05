﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Board2Make.Model
{
    class Model
    {
        public ValidatedPath boardTxt { get; set; }
        public ValidatedPath compilerBase { get; set; }
        public ValidatedPath projectBase { get; set; }
        public ValidatedPath arduinoBase { get; set; }
        public ValidatedPath coreBase { get; set; }
        public ValidatedPath makeExe { get; set; }
        public ValidatedPath uplTyBase { get; set; }
        public ValidatedPath uplPjrcBase { get; set; }


        public string makefile { get; private set; }
        public string tasks_json { get; private set; }
        public string propsFile { get; private set; }

        public List<Board> boards { get; private set; } = new List<Board>();

        public void parseBoardsTxt(bool fromArduino)
        {
            Console.WriteLine("parseBoardsTxt");

            string boardFile = fromArduino ?
                 (arduinoBase.hasError ? null : FileHelpers.getBoardFromArduino(arduinoBase.path)) :
                 (boardTxt.hasError ? null : boardTxt.path);

            boards.Clear();

            var content = new FileContent(boardFile);
            if (!content.ParseError)
            {
                boards = content.boards.Where(b => b.core.ToUpper() == "TEENSY3").ToList();
            }
        }
        public void generateFiles(Board board, bool fromArduino = true)
        {
            makefile = tasks_json = propsFile = null;

            if (board != null)
            {
                var options = board.getAllOptions();

                if (fromArduino)
                {
                    if (arduinoBase.isValid && uplTyBase.isValid)
                    {
                        Console.WriteLine("generate files (quick)");
                        string compilerPath = Path.Combine(FileHelpers.getToolsFromArduino(arduinoBase.path), "arm");
                        string corePath = FileHelpers.getCoreFromArduino(arduinoBase.path);
                        string uplPjcrPath = FileHelpers.getToolsFromArduino(arduinoBase.path);

                        makefile = generateMakefile(board.name, board.optionSets, options, compilerPath, corePath, uplTyBase.path, uplPjcrPath);
                        propsFile = generatePropertiesFile(options, compilerPath, corePath);
                    }
                }
                else
                {
                    if (compilerBase.isValid && coreBase.isValid && uplPjrcBase.isValid && uplTyBase.isValid)
                    {
                        Console.WriteLine("generate files");
                        string compilerPath = compilerBase.path;
                        string corePath = coreBase.path;

                        makefile = generateMakefile(board.name, board.optionSets, options, compilerPath, corePath, uplTyBase.path, uplPjrcBase.path);
                        propsFile = generatePropertiesFile(options, compilerPath, corePath);
                    }
                }
            }
            if (makeExe.isValid)
            {
                tasks_json = generateTasksFile(makeExe.path);
            }
        }

        public Model()
        {
            projectBase = new ValidatedPath(path => String.IsNullOrWhiteSpace(path) ? null : (Directory.Exists(path) ? null : "not found"));
            arduinoBase = new ValidatedPath(path => Directory.Exists(path) ? null : "not found");
            boardTxt = new ValidatedPath(path => File.Exists(path) ? null : "not found");
            coreBase = new ValidatedPath(path => Directory.Exists(path) ? null : "not found");
            compilerBase = new ValidatedPath(path => Directory.Exists(path) ? null : "not found");
            makeExe = new ValidatedPath(path => File.Exists(path) ? null : "make not found") { path = @"c:\toolchain\make\make.exe" };
            uplTyBase = new ValidatedPath(path => Directory.Exists(path) ? null : "TyTools not found") { path = @"c:\toolchain\TyTools" };

            arduinoBase.path = @"c:\arduino\arduino-1.8.5";
        }

        private string generateTasksFile(string makePath)
        {
            var tasks = new tasksJson()
            {
                presentation = new Presentation(),
                tasks = new List<Task>()
                {
                    new Task()
                    {
                        label = "Build",
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
        private string generatePropertiesFile(Dictionary<string, string> options, string compilerPath, string corePath)
        {
            var props = new PropertiesJson()
            {
                configurations = new List<Configuration>()
                {
                    new Configuration()
                    {
                        name = "VisualTeensy",
                        compilerPath =  Path.Combine(compilerPath ,"bin","arm-none-eabi-gcc.exe").Replace('\\','/'),
                        intelliSenseMode = "gcc-x64",
                        includePath = new List<string>()
                        {
                            "${workspaceFolder}/src/**",
                            corePath?.Replace('\\','/') + "/**"
                        },
                        defines = new List<string>()
                    }
                }
            };

            foreach (var define in options["build.flags.defs"].Split(new string[] { "-D" }, StringSplitOptions.RemoveEmptyEntries))
            {
                props.configurations[0].defines.Add(define.Trim());
            }

            props.configurations[0].defines.Add("F_CPU=" + options["build.fcpu"]);
            props.configurations[0].defines.Add(options["build.usbtype"]);
            props.configurations[0].defines.Add("LAYOUT_" + options["build.keylayout"]);

            return FileHelpers.FormatOutput(new JavaScriptSerializer().Serialize(props));
        }
        private string generateMakefile(string boardName, List<OptionSet> optionSets, Dictionary<string, string> options, string compilerPath, string corePath, string uplTyPath, string uplPjrcPath)
        {
            StringBuilder mf = new StringBuilder();

            mf.Append("#******************************************************************************\n");
            mf.Append("# Generated by Board2Make (https://github.com/luni64/Board2Make)\n");
            mf.Append("#\n");
            mf.Append($"# {"Board",-18} {boardName}\n");
            optionSets.ForEach(o => mf.Append($"# {o.name,-18} {o.selectedOption.name}\n"));
            mf.Append("#\n");
            mf.Append($"# {"Boards.txt",-18} {boardTxt.path}\n");

            mf.Append("#\n");
            mf.Append($"# {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}\n");
            mf.Append("#******************************************************************************\n\n");

            mf.Append("TARGET_NAME := newtest\n\n");

            mf.Append(makeEntry("BOARD_ID    := ", "build.board", options) + "\n");
            mf.Append($"CORE_BASE   := {corePath}\n");
            mf.Append($"GCC_BASE    := {compilerPath}\n");
            mf.Append($"UPL_PJRC_B  := {uplPjrcPath}\n");
            mf.Append($"UPL_TYCMD_B := {uplTyPath}\n\n");
         
            mf.Append(makeEntry("FLAGS_CPU   := ", "build.flags.cpu", options) + "\n");
            mf.Append(makeEntry("FLAGS_OPT   := ", "build.flags.optimize", options) + "\n");
            mf.Append(makeEntry("FLAGS_COM   := ", "build.flags.common", options) + makeEntry(" ", "build.flags.dep", options) + "\n");
            mf.Append(makeEntry("FLAGS_LSP   := ", "build.flags.ldspecs", options) + "\n");

            mf.Append("\n");
            mf.Append(makeEntry("FLAGS_CPP   := ", "build.flags.cpp", options) + "\n");
            mf.Append(makeEntry("FLAGS_C     := ", "build.flags.c", options) + "\n");
            mf.Append(makeEntry("FLAGS_S     := ", "build.flags.S", options) + "\n");
            mf.Append(makeEntry("FLAGS_LD    := ", "build.flags.ld", options) + "\n");

            mf.Append("\n");
            mf.Append(makeEntry("LIBS        := ", "build.flags.libs", options) + "\n");
            mf.Append(makeEntry("LD_SCRIPT   := ", "build.mcu", options) + ".ld\n");

            mf.Append("\n");
            mf.Append(makeEntry("DEFINES     := ", "build.flags.defs", options) + "\n");
            mf.Append("DEFINES     += ");
            mf.Append(makeEntry("-DF_CPU=", "build.fcpu", options) + " " + makeEntry("-D", "build.usbtype", options) + " " + makeEntry("-DLAYOUT_", "build.keylayout", options) + "\n");

            mf.Append($"\n");
            mf.Append("CPP_FLAGS   := $(FLAGS_CPU) $(FLAGS_OPT) $(FLAGS_COM) $(DEFINES) $(FLAGS_CPP)\n");
            mf.Append("C_FLAGS     := $(FLAGS_CPU) $(FLAGS_OPT) $(FLAGS_COM) $(DEFINES) $(FLAGS_C)\n");
            mf.Append("S_FLAGS     := $(FLAGS_CPU) $(FLAGS_OPT) $(FLAGS_COM) $(DEFINES) $(FLAGS_S)\n");
            mf.Append("LD_FLAGS    := $(FLAGS_CPU) $(FLAGS_OPT) $(FLAGS_LSP) $(FLAGS_LD)\n");
            mf.Append("AR_FLAGS    := rcs\n");
            mf.Append(mkf);

            return mf.ToString();
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
               "# The function rwildcard is taken from http://stackoverflow.com/a/12959694)\n\n" +

               "rwildcard =$(wildcard $1$2) $(foreach d,$(wildcard $1*),$(call rwildcard,$d/,$2))\n\n" +

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
               "#$(info INFO: ${CORE_OBJ})\n\n" +

               "INCLUDE        := -I.\\$(USR_SRC) -I$(CORE_SRC)\n\n\n" +


               "#******************************************************************************\n" +
               "# Rules:\n" +
               "#******************************************************************************\n" +
               "\n" +
               ".PHONY: all rebuild upload uploadTy clean cleanUser cleanCore\n\n" +

               "all:     $(TARGET_LST) $(TARGET_HEX)\n" +
               "rebuild: cleanUser all\n" +
               "clean:   cleanUser cleanCore\n\n" +

               "upload: $(TARGET_LST) $(TARGET_HEX)\n" +
                  "\t@$(UPL_PJRC)\n\n" +

               "uploadTy: $(TARGET_LST) $(TARGET_HEX)\n" +
                  "\t@$(UPL_TYCMD)\n\n" +


               "# Core library ----------------------------------------------------------------\n" +
               "$(CORE_BIN)/%.o: $(CORE_SRC)/%.S\n" +
                  "\t@echo [ASM] CORE $(notdir $<)\n" +
                  "\t@if not exist $(dir $@)  @mkdir \"$(dir $@)\"\n" +
                  "\t@\"$(CC)\" $(S_FLAGS) $(INCLUDE) -o $@ -c $< \n\n" +

               "$(CORE_BIN)/%.o: $(CORE_SRC)/%.c\n" +
                  "\t@echo [CC]  CORE $(notdir $<)\n" +
                  "\t@if not exist $(dir $@)  @mkdir \"$(dir $@)\"\n" +
                  "\t@\"$(CC)\" $(C_FLAGS) $(INCLUDE) -o $@ -c $< \n\n" +

               "$(CORE_BIN)/%.o: $(CORE_SRC)/%.cpp\n" +
                  "\t@echo [CPP] CORE $(notdir $<)\n" +
                  "\t@if not exist $(dir $@)  @mkdir \"$(dir $@)\"\n" +
                  "\t@\"$(CXX)\" $(CPP_FLAGS) $(INCLUDE) -o $@ -c $< \n\n" +

               "$(CORE_LIB) : $(CORE_OBJ)\n" +
                  "\t@echo [AR]  $@\n" +
                  "\t@$(AR) $(AR_FLAGS) $@ $^\n" +
                  "\t@echo Teensy core built successfully &&echo.\n\n" +


               "# Handle user sources ---------------------------------------------------------\n" +
                  "$(USR_BIN)/%.o: $(USR_SRC)/%.S\n" +
                  "\t@echo [ASM] $<\n" +
                  "\t@if not exist $(dir $@)  @mkdir \"$(dir $@)\"\n" +
                  "\t@\"$(CC)\" $(S_FLAGS) $(INCLUDE) -o $@ -c $<\n\n" +

               "$(USR_BIN)/%.o: $(USR_SRC)/%.c\n" +
                  "\t@echo [CC]  $(notdir $<)\n" +
                  "\t@if not exist $(dir $@)  @mkdir \"$(dir $@)\"\n" +
                  "\t@\"$(CC)\" $(C_FLAGS) $(INCLUDE) -o \"$@\" -c $<\n\n" +

               "$(USR_BIN)/%.o: $(USR_SRC)/%.cpp\n" +
                  "\t@echo [CPP] $<\n" +
                  "\t@if not exist $(dir $@)  @mkdir \"$(dir $@)\"\n" +
                  "\t@\"$(CXX)\" $(CPP_FLAGS) $(INCLUDE) -o \"$@\" -c $<\n\n" +


               "# Linking ---------------------------------------------------------------------\n" +
               "$(TARGET_ELF): $(CORE_LIB) $(USR_OBJ)\n" +
                  "\t@echo [LD]  $@\n" +
                  "\t@$(CC) $(LD_FLAGS) -T$(CORE_SRC)/$(LD_SCRIPT) -o \"$@\" $(USR_OBJ) $(CORE_LIB) $(LIBS)\n" +
                  "\t@echo User code built and linked to core lib &&echo.\n\n" +

               "%.lst: %.elf\n" +
                  "\t@echo [LST] $@\n" +
                  "\t@$(OBJDUMP) -d -S --demangle --no-show-raw-insn --syms \"$<\"  > \"$@\"\n" +
                  "\t@echo Listfile generated &&echo.\n\n" +

               "%.hex: %.elf\n" +
                  "\t@echo [HEX] $@\n" +
                  "\t@$(SIZE) \"$<\"\n" +
                  "\t@$(OBJCOPY) -O ihex -R.eeprom \"$<\" \"$@\"\n" +
                  "\t@echo Sucessfully built project &&echo.\n\n" +


               "# Cleaning --------------------------------------------------------------------\n" +
               "cleanUser:\n" +
                  "\t@echo Cleaning user binaries...\n" +
                  "\t@if exist $(USR_BIN) rd /s/q \"$(USR_BIN)\"\n\n" +

               "\t@if exist $(TARGET_HEX) del  $(TARGET_HEX)\n" +
                  "\t@if exist $(TARGET_ELF) del  $(TARGET_ELF)\n" +
                  "\t@if exist $(TARGET_LST) del  $(TARGET_LST)\n\n" +

               "cleanCore:\n" +
                  "\t@echo Cleaning core binaries...\n" +
                  "\t@if exist $(CORE_BIN) rd /s/q \"$(CORE_BIN)\"\n" +
                  "\t@if exist $(CORE_LIB) del  \"$(CORE_LIB)\"\n\n" +


               "# compiler generated dependency info ------------------------------------------\n" +
               "-include $(CORE_OBJ:.o=.d)\n" +
               "-include $(USR_OBJ:.o=.d)\n";

        private string makeEntry(String txt, String key, Dictionary<String, String> options)
        {
            if (options.ContainsKey(key))
            {
                return $"{txt}{options[key]}";
            }
            else
            {
                return "";
            }
        }
    }
}
