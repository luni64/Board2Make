using Microsoft.Win32;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace ViewModel
{
    public class OptionSetVM : BaseViewModel
    {
        #region Properties ----------------------------------
        public String displayName
        {
            get { return _displayName; }
            set { SetProperty(ref _displayName, value); }
        }
        String _displayName;

        public String optionSetID
        {
            get { return _optionSetID; }
            set { SetProperty(ref _optionSetID, value); }
        }
        String _optionSetID;

        public Option selectedOption
        {
            get { return _selectedOption; }
            set
            {
                SetProperty(ref _selectedOption, value);
            }
        }
        Option _selectedOption;

        public ObservableCollection<Option> Options { get; set; }
        #endregion

        public OptionSetVM(String optionSetID, String displayName, IEnumerable<Option> options)
        {
            this.displayName = displayName;
            this.optionSetID = optionSetID;
            this.Options = new ObservableCollection<Option>(options.ToList());
            this.selectedOption = Options.FirstOrDefault();
        }
    }

    public class BoardVM : BaseViewModel
    {
        #region Properties ---------------------------------------------
        public String boardName { get; private set; }

        public String makefile
        {
            get { return _makefile; }
            set { SetProperty(ref _makefile, value); }
        }
        String _makefile;

        public ObservableCollection<BuildEntry> boardOptions;

        public ObservableCollection<OptionSetVM> optionSets
        {
            get { return _optionSets; }
            set { SetProperty(ref _optionSets, value); }
        }
        ObservableCollection<OptionSetVM> _optionSets;

        #endregion

        public BoardVM(Board board, IEnumerable<Menu> menus)
        {
            boardName = board.name;
            optionSets = new ObservableCollection<OptionSetVM>();
            boardOptions = new ObservableCollection<BuildEntry>(board.fixedOptions);

            foreach (var optionSet in board.optionSets)
            {
                var menuName = menus.FirstOrDefault(m => m.OptionSetID == optionSet.Key).MenuName ?? "UNKNOWN";
                var optionSetVm = new OptionSetVM(optionSet.Key, menuName, optionSet.Value);
                optionSetVm.PropertyChanged += (s, e) => OnPropertyChanged("allOptions");
                optionSets.Add(optionSetVm);
            }
        }


        public Dictionary<String, String> getAllOptions()
        {
            Dictionary<string, string> allOptions = boardOptions.ToDictionary(o => o.name, o => o.value);

            foreach (var optionSet in optionSets)
            {
                var options = optionSet?.selectedOption?.paramList;
                if (options != null)
                {
                    foreach (var option in options)
                    {
                        if (allOptions.ContainsKey(option.name))
                        {
                            allOptions[option.name] = option.value;  // overwrite defaults if necessary
                        }
                        else
                        {
                            allOptions.Add(option.name, option.value);
                        }
                    }
                }
            }

            //Hack, would be better to change the build.flags.ld entry instead of replacing
            allOptions.Remove("build.flags.ld");
            allOptions.Add("build.flags.ld", "-Wl,--gc-sections,--relax,--defsym=__rtc_localtime=0");

            return allOptions;
        }
    }

    public class ViewModel : BaseViewModel
    {
        public RelayCommand cmdSave { get; private set; }
        void doCmdSave(object o)
        {
            if (outputFilename != null)
            {
                using (TextWriter writer = new StreamWriter(outputFilename))
                {
                    writer.Write(makefile);
                }
            }
        }


        public String inputFilename
        {
            get { return _inputFilename; }
            set
            {
                if (_inputFilename != value)
                {
                    SetProperty(ref _inputFilename, value);
                    _inputFilename = value;
                    RegistryKey key = Registry.CurrentUser.CreateSubKey("Software\\lunOptics\\Board2Make");
                    key.SetValue("input", _inputFilename);
                    openFile();
                }
            }
        }
        String _inputFilename;

        public String outputFilename
        {
            get { return _outputFilename; }
            set
            {
                if (_outputFilename != value)
                {
                    _outputFilename = value;
                    RegistryKey key = Registry.CurrentUser.CreateSubKey("Software\\lunOptics\\Board2Make");
                    key.SetValue("output", _outputFilename);
                }
            }
        }
        String _outputFilename;


        #region Properties ------------------------------------------------------
        public String Title { get { return "lunOptics - Board2Make"; } }

        public ObservableCollection<BoardVM> boardVMs { get; } = new ObservableCollection<BoardVM>();

        public BoardVM selectedBoard
        {
            get { return _selectedBoard; }
            set
            {
                SetProperty(ref _selectedBoard, value);
                makefile = _selectedBoard != null ? makeMakefile() : "Please select a board";
            }
        }
        BoardVM _selectedBoard;

        public String makefile
        {
            get { return _makefile; }
            set { SetProperty(ref _makefile, value); }
        }
        String _makefile;

        #endregion

        public void openFile()
        {
            if (File.Exists(inputFilename))
            {
                foreach (var board in boardVMs) board.PropertyChanged -= (s, e) => makeMakefile(); // remove old event handlers
                boardVMs.Clear();

                var content = new FileContent(inputFilename);
                if (!content.ParseError)
                { 
                    foreach (var board in content.boards.Where(b => b.core.ToUpper() == "TEENSY3"))
                    {
                        var boardVM = new BoardVM(board, content.menus);
                        boardVM.PropertyChanged += (s, e) => makefile = makeMakefile();
                        boardVMs.Add(boardVM);
                    }
                    selectedBoard = boardVMs.FirstOrDefault();

                   // makefile = selectedBoard != null ? makeMakefile() : "Input file does not contain boards with teensy3 core.";
                }
                else makefile = "Error parsing the input file!\nPlease check if it is a valid boards.txt file";
            }
            else makefile = "Can't open the input file! Please select an existing teensy board.txt. \nThe file is typically found here: 'Arduino\\hardware\\teensy\\avr\\boards.txt'";
        }
        
        public ViewModel()
        {
            cmdSave = new RelayCommand(doCmdSave);

            var key = Registry.CurrentUser.OpenSubKey("Software\\lunOptics\\Board2Make");
            _inputFilename = key?.GetValue("input") as String;
            _outputFilename = key?.GetValue("output") as String;


            openFile();

        }

        string makeMakefile()
        {
            var ao = selectedBoard.getAllOptions();

            StringBuilder mf = new StringBuilder();
            mf.Append("#*****************************************************************\n");
            mf.Append("# Generated by Board2Make (https://github.com/luni64/Board2Make)\n");
            mf.Append("#\n");
            mf.Append($"# {"Board",-18} {selectedBoard.boardName}\n");
            foreach(var os in selectedBoard.optionSets)
            {
                mf.Append($"# {os.displayName,-18} {os.selectedOption.optionID}\n");
            }
            mf.Append("#\n");
            mf.Append($"# {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}\n");
            mf.Append("#*****************************************************************\n\n");

            mf.Append(makeEntry("BOARD_ID   := ", "build.board", ao) + "\n");
            mf.Append($"\n");

            mf.Append(makeEntry("FLAGS_CPU  := ", "build.flags.cpu", ao) + "\n");
            mf.Append(makeEntry("FLAGS_OPT  := ", "build.flags.optimize", ao) + "\n");
            mf.Append(makeEntry("FLAGS_COM  := ", "build.flags.common", ao) + makeEntry(" ", "build.flags.dep", ao) + "\n");
            mf.Append(makeEntry("FLAGS_LSP  := ", "build.flags.ldspecs", ao) + "\n");

            mf.Append("\n");
            mf.Append(makeEntry("FLAGS_CPP  := ", "build.flags.cpp", ao) + "\n");
            mf.Append(makeEntry("FLAGS_C    := ", "build.flags.c", ao) + "\n");
            mf.Append(makeEntry("FLAGS_S    := ", "build.flags.S", ao) + "\n");
            mf.Append(makeEntry("FLAGS_LD   := ", "build.flags.ld", ao) + "\n");

            mf.Append("\n");
            mf.Append(makeEntry("LIBS       := ", "build.flags.libs", ao) + "\n");
            mf.Append(makeEntry("LD_SCRIPT  := ", "build.mcu", ao) + ".ld\n");

            mf.Append("\n");
            mf.Append(makeEntry("DEFINES    := ", "build.flags.defs", ao) + "\n");
            mf.Append("DEFINES    += ");
            mf.Append(makeEntry("-DF_CPU=", "build.fcpu", ao) + " " + makeEntry("-D", "build.usbtype", ao) + " " + makeEntry("-DLAYOUT_", "build.keylayout", ao) + "\n");

            mf.Append($"\n");
            mf.Append("CPP_FLAGS  := $(FLAGS_CPU) $(FLAGS_OPT) $(FLAGS_COM) $(DEFINES) $(FLAGS_CPP)\n");
            mf.Append("C_FLAGS    := $(FLAGS_CPU) $(FLAGS_OPT) $(FLAGS_COM) $(DEFINES) $(FLAGS_C)\n");
            mf.Append("S_FLAGS    := $(FLAGS_CPU) $(FLAGS_OPT) $(FLAGS_COM) $(DEFINES) $(FLAGS_S)\n");
            mf.Append("LD_FLAGS   := $(FLAGS_CPU) $(FLAGS_OPT) $(FLAGS_LSP) $(FLAGS_LD)\n");
            mf.Append("AR_FLAGS   := rcs\n");


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
    }
}


