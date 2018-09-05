using Board2Make.Model;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ViewModel
{
    public class ViewModel : BaseViewModel, IDataErrorInfo
    {
        public RelayCommand cmdSave { get; private set; }
        void doCmdSave(object o)
        {
            if (model.projectBase.isValid)
            {
                string projectPath = model.projectBase.path;
                string settingsPath = Path.Combine(projectPath, ".vscode");
                string srcPath = Path.Combine(projectPath, "src");

                Directory.CreateDirectory(settingsPath);
                Directory.CreateDirectory(srcPath);

                string makefilePath = Path.Combine(projectPath, "makefile");
                using (TextWriter writer = new StreamWriter(makefilePath))
                {
                    writer.Write(makefile);
                }

                string taskFilePath = Path.Combine(settingsPath, "tasks.json");
                using (TextWriter writer = new StreamWriter(taskFilePath))
                {
                    writer.Write(taskFile);
                }

                string propFilePath = Path.Combine(settingsPath, "c_cpp_properties.json");
                using (TextWriter writer = new StreamWriter(propFilePath))
                {
                    writer.Write(propFile);
                }
                                
                string mainPath = Path.Combine(srcPath, "main.cpp");
                
                using (TextWriter writer = new StreamWriter(mainPath))
                {
                    writer.Write(mainCpp);
                }



            }

        }


        const string mainCpp =
            "#include \"arduino.h\"\n\n" +
            "void setup()\n" +
            "{\n" +
            "\tpinMode(LED_BUILTIN,OUTPUT);\n" +
            "}\n\n" +

            "void loop()\n" +
            "{\n" +
                "\tdigitalWriteFast(LED_BUILTIN,!digitalReadFast(LED_BUILTIN));\n" +
                "\tdelay(250);\n" +
            "}\n";


        #region IDataErrorInfo ------------------------------------------------

        public string Error => throw new NotImplementedException();

        public string this[string columnName]
        {
            get
            {
                string error;

                switch (columnName)
                {
                    case "projectPath":
                        error = model.projectBase.ValidationResult;
                        break;

                    case "arduinoPath":
                        error = quickSetup ? model.arduinoBase.ValidationResult : null;
                        break;

                    case "boardTxtPath":
                        error = quickSetup ? null : model.boardTxt.ValidationResult;
                        break;

                    case "corePath":
                        error = quickSetup ? null : model.coreBase.ValidationResult;
                        break;

                    case "compilerPath":
                        error = quickSetup ? null : model.compilerBase.ValidationResult;
                        break;

                    case "makePath":
                        error = model.makeExe.ValidationResult;
                        break;

                    case "uploadTyPath":
                        error = String.IsNullOrEmpty(uploadTyPath) ? null : model.uplTyBase.ValidationResult;
                        break;

                    default:
                        error = null;
                        break;
                }
                return error;


            }
        }
        #endregion


        #region Properties ------------------------------------------------------
        public String makefile => model.makefile;
        public String propFile => model.propsFile;
        public String taskFile => model.tasks_json;

        public String makeFileName => Path.Combine(projectPath ?? "", "makefile");
        public String propFileName => Path.Combine(projectPath ?? "", ".vscode", "c_cpp_properties.json");
        public String taskFileName => Path.Combine(projectPath ?? "", ".vscode", "tasks.json");

        public String projectPath
        {
            get => model.projectBase.path;
            set
            {
                if (value != model.projectBase.path)
                {
                    model.projectBase.path = value.Trim();
                    OnPropertyChanged();
                    OnPropertyChanged("makeFileName");
                    OnPropertyChanged("propFileName");
                    OnPropertyChanged("taskFileName");
                }
            }
        }
        public String arduinoPath
        {
            get => model.arduinoBase.path;
            set
            {
                if (value != model.arduinoBase.path)
                {
                    model.arduinoBase.path = value.Trim();
                    OnPropertyChanged();
                    updateBoards();
                }
            }
        }
        public String boardTxtPath
        {
            get => model.boardTxt.path;
            set
            {
                if (value != model.boardTxt.path)
                {
                    model.boardTxt.path = value.Trim();
                    OnPropertyChanged();
                    updateBoards();
                }
            }
        }
        public String corePath
        {
            get => model.coreBase.path;
            set
            {
                if (value != model.coreBase.path)
                {
                    model.coreBase.path = value.Trim();
                    OnPropertyChanged();
                    updateFiles();
                }

            }
        }
        public String compilerPath
        {
            get => model.compilerBase.path;
            set
            {
                if (value != model.compilerBase.path)
                {
                    model.compilerBase.path = value.Trim();
                    OnPropertyChanged();
                    updateFiles();
                }
            }
        }
        public String makePath
        {
            get => model.makeExe.path;
            set
            {
                if (value != model.makeExe.path)
                {
                    model.makeExe.path = value.Trim();
                    OnPropertyChanged();
                    updateFiles();
                }
            }
        }


        public String uploadTyPath
        {
            get => model.uplTyBase.path;
            set
            {
                if(value != model.uplTyBase.path)
                {
                    model.uplTyBase.path = value;
                    OnPropertyChanged();
                }
            }
        }
        string _uploadTyPath;

        public String uploadPjrcPath
        {
            get => _uploadPjrcPath;
            set => SetProperty(ref _uploadPjrcPath, value);
        }
        string _uploadPjrcPath;

        public bool quickSetup
        {
            get => _quickSetup;
            set
            {
                SetProperty(ref _quickSetup, value);
                updateBoards();
                OnPropertyChanged("");
            }
        }
        bool _quickSetup = true;

        public String outputFilename
        {
            get => _outputFilename;
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

        public String Title => "lunOptics - VisualTeensy V0.1";

        public ObservableCollection<BoardVM> boardVMs { get; } = new ObservableCollection<BoardVM>();

        public BoardVM selectedBoard
        {
            get => _selectedBoard;
            set
            {
                if (value != _selectedBoard)
                {
                    _selectedBoard = value;
                    OnPropertyChanged();
                    updateFiles();
                }
            }
        }
        BoardVM _selectedBoard;


        #endregion

        public void updateFiles()
        {
            model.generateFiles(selectedBoard?.board, quickSetup);
            OnPropertyChanged("makefile");
            OnPropertyChanged("propFile");
            OnPropertyChanged("taskFile");
        }


        public void updateBoards()
        {
            model.parseBoardsTxt(quickSetup);

            foreach (var boardVM in boardVMs)  // remove old event handlers
            {
                foreach (var optionSetVM in boardVM.optionSetVMs)
                {
                    optionSetVM.PropertyChanged -= (s, e) => updateFiles();
                }
            }
            boardVMs.Clear();

            foreach (var board in model.boards)
            {
                var boardVM = new BoardVM(board);
                boardVMs.Add(boardVM);
                foreach (var optionSetVM in boardVM.optionSetVMs)
                {
                    optionSetVM.PropertyChanged += (s, e) => updateFiles();
                }
            }
            selectedBoard = boardVMs.FirstOrDefault();
        }


        public ViewModel()
        {
            if (Debugger.IsAttached)
            {
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            }

            cmdSave = new RelayCommand(doCmdSave);

            updateBoards();
        }



        private Model model = new Model();
    }
}


