using Board2Make.Model;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ViewModel
{
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

        #region Properties ------------------------------------------------------
        public String makefile
        {
            get => _makefile;
            set => SetProperty(ref _makefile, value);
        }
        String _makefile;

        public String propFile
        {
            get => _propFile;
            set => SetProperty(ref _propFile, value);
        }
        String _propFile;

        public String taskFile
        {
            get => _taskFile;
            set => SetProperty(ref _taskFile, value);
        }
        String _taskFile;

        public String makeFileName => Path.Combine(projectPath ?? "", "makefile");
        public String propFileName => Path.Combine(projectPath ?? "", ".vscode", "c_cpp_properties.json");
        public String taskFileName => Path.Combine(projectPath ?? "", ".vscode", "tasks.json");


        public String boardTxt_filename
        {
            get { return _boardTxt_Filename; }
            set
            {
                if (_boardTxt_Filename != value)
                {
                    SetProperty(ref _boardTxt_Filename, value);
                    openFile();
                }
            }
        }
        String _boardTxt_Filename;

        public String arduinoPath
        {
            get => _arduinoPath;
            set => SetProperty(ref _arduinoPath, value);
        }
        string _arduinoPath = "C:\\Arduino\\arduino-1.8.5";

        public String compilerPath
        {
            get => _compilerPath;
            set => SetProperty(ref _compilerPath, value);
        }
        string _compilerPath;

        public String corePath
        {
            get => _corePath;
            set => SetProperty(ref _corePath, value);
        }
        string _corePath;

        public String projectPath
        {
            get => _projectPath;
            set
            {
                SetProperty(ref _projectPath, value);
                OnPropertyChanged("makeFileName");
                OnPropertyChanged("propFileName");
                OnPropertyChanged("taskFileName");
            }
        }
        string _projectPath;

        public String makePath
        {
            get => _makePath;
            set => SetProperty(ref _makePath, value);
        }
        string _makePath;

        public String uploadTyPath
        {
            get => _uploadTyPath;
            set => SetProperty(ref _uploadTyPath, value);
        }
        string _uploadTyPath;

        public String uploadPjrcPath
        {
            get => _uploadPjrcPath;
            set => SetProperty(ref _uploadTyPath, value);
        }
        string _uploadPjrcPath;

        public bool quickSetup
        {
            get => _quickSetup;
            set => SetProperty(ref _quickSetup, value);
        }
        bool _quickSetup = true;

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

        public String Title => "lunOptics - Board2Make";

        public ObservableCollection<BoardVM> boardVMs { get; } = new ObservableCollection<BoardVM>();

        public BoardVM selectedBoard
        {
            get { return _selectedBoard; }
            set
            {
                SetProperty(ref _selectedBoard, value);
                //makefile = model.generateMakefile(selectedBoard?.board);
            }
        }
        BoardVM _selectedBoard;


        #endregion

        public void openFile()
        {
            model.parseBoardsTxt(boardTxt_filename);

            foreach (var boardVM in boardVMs)  // remove old event handlers
            {
                foreach (var optionSetVM in boardVM.optionSetVMs)
                {
                    optionSetVM.PropertyChanged -= (s, e) => makefile = model.generateMakefile(selectedBoard.board);
                }
            }

            boardVMs.Clear();

            foreach (var board in model.boards/*.Where(b => b.core.ToUpper() == "TEENSY3")*/)
            {
                var boardVM = new BoardVM(board);
                boardVMs.Add(boardVM);
                foreach (var optionSetVM in boardVM.optionSetVMs)
                {
                    optionSetVM.PropertyChanged += (s, e) => ViewModel_PropertyChanged(s, e);// makefile = model.generateMakefile(selectedBoard.board);
                }
            }
            selectedBoard = boardVMs.FirstOrDefault();
        }


        public ViewModel()
        {
            if (Debugger.IsAttached) CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");

            cmdSave = new RelayCommand(doCmdSave);

            var key = Registry.CurrentUser.OpenSubKey("Software\\lunOptics\\Board2Make");
            _boardTxt_Filename = key?.GetValue("input") as String;
            _outputFilename = key?.GetValue("output") as String;

            model = new Model(boardTxt_filename);
            openFile();

            PropertyChanged += ViewModel_PropertyChanged;

            OnPropertyChanged("quickSetup");
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "makefile":
                    break;

                default:
                    if (quickSetup == true)
                    {
                        model.boardTxtPath = FileHelpers.getBoardFromArduino(arduinoPath);
                        model.corePath = FileHelpers.getCoreFromArduino(arduinoPath);
                        var toolsPath = FileHelpers.getToolsFromArduino(arduinoPath);
                        model.compilerPath = Path.Combine(toolsPath, "arm");
                        model.uploadPathTeensy = toolsPath;
                    }
                    else
                    {
                        model.boardTxtPath = boardTxt_filename;
                        model.compilerPath = compilerPath;
                        model.corePath = corePath;
                        model.uploadPathTeensy = null;
                    }
                    model.uploadPathTY = uploadTyPath;
                    model.makePath = makePath;

                    makefile = model.generateMakefile(selectedBoard?.board);
                    propFile = model.generatePropertiesFile();
                    taskFile = model.generateTasksFile();
                    break;

            }

        }

        private Model model;
    }
}


