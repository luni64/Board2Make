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
        public String makefile => model.makefile;
        public String propFile => model.propsFile;
        public String taskFile => model.tasks_json;

        public String makeFileName => Path.Combine(projectPath ?? "", "makefile");
        public String propFileName => Path.Combine(projectPath ?? "", ".vscode", "c_cpp_properties.json");
        public String taskFileName => Path.Combine(projectPath ?? "", ".vscode", "tasks.json");

        public String boardTxtPath
        {
            get => model.boardTxt.path;
            set
            {
                if (value != model.boardTxt.path)
                {
                    model.boardTxt.path = value.Trim();
                    OnPropertyChanged();
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
                }

            }
        }
        public String projectPath
        {
            get => model.projectBase.path;
            set
            {
                if (value != model.projectBase.path)
                {
                    model.projectBase.path = value;
                    OnPropertyChanged();
                    OnPropertyChanged("makeFileName");
                    OnPropertyChanged("propFileName");
                    OnPropertyChanged("taskFileName");
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
                }
            }
        }


        public String uploadTyPath
        {
            get => _uploadTyPath;
            set => SetProperty(ref _uploadTyPath, value);
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
            set => SetProperty(ref _quickSetup, value);
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

        public String Title => "lunOptics - Board2Make";

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
                }
            }
        }
        BoardVM _selectedBoard;


        #endregion

        public void updateBoards()
        {
            model.parseBoardsTxt(quickSetup);

            foreach (var boardVM in boardVMs)  // remove old event handlers
            {
                foreach (var optionSetVM in boardVM.optionSetVMs)
                {
                    optionSetVM.PropertyChanged -= (s, e) => ViewModel_PropertyChanged(s, e);
                }
            }
            boardVMs.Clear();

            foreach (var board in model.boards)
            {
                var boardVM = new BoardVM(board);
                boardVMs.Add(boardVM);
                foreach (var optionSetVM in boardVM.optionSetVMs)
                {
                    optionSetVM.PropertyChanged += (s, e) => ViewModel_PropertyChanged(s, e);
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
            PropertyChanged += ViewModel_PropertyChanged;
            updateBoards();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "arduinoPath":
                case "boardTxtPath":
                    updateBoards();
                    break;

                case "selectedOption":
                case "selectedBoard":
                case "quickSetup":
                case "corePath":
                case "compilerPath":
                    model.generateFiles(selectedBoard?.board, quickSetup);
                    OnPropertyChanged("makefile");
                    OnPropertyChanged("propFile");
                    OnPropertyChanged("taskFile");
                    break;
            }
        }

        private Model model = new Model();
    }
}


