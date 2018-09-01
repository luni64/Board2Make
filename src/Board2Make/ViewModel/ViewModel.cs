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
        public String inputFilename
        {
            get { return _inputFilename; }
            set
            {
                if (_inputFilename != value)
                {
                    SetProperty(ref _inputFilename, value);
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
        
        public String Title { get { return "lunOptics - Board2Make"; } }

        public ObservableCollection<BoardVM> boardVMs { get; } = new ObservableCollection<BoardVM>();

        public BoardVM selectedBoard
        {
            get { return _selectedBoard; }
            set
            {
                SetProperty(ref _selectedBoard, value);
                makefile = model.generateMakefile(selectedBoard?.board);
            }
        }
        BoardVM _selectedBoard;

        public String makefile
        {
            get => _makefile;
            set => SetProperty(ref _makefile, value);
        }
        String _makefile;

        #endregion

        public void openFile()
        {
            model.parseBoardsTxt(inputFilename);

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
                    optionSetVM.PropertyChanged += (s,e)=> makefile = model.generateMakefile(selectedBoard.board);
                }
            }
            selectedBoard = boardVMs.FirstOrDefault();
        }
              

        public ViewModel()
        {
            if (Debugger.IsAttached) CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");

            cmdSave = new RelayCommand(doCmdSave);

            var key = Registry.CurrentUser.OpenSubKey("Software\\lunOptics\\Board2Make");
            _inputFilename = key?.GetValue("input") as String;
            _outputFilename = key?.GetValue("output") as String;

            model = new Model(inputFilename);
            openFile();
        }

        private Model model;
    }
}


