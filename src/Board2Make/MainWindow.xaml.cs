using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using ViewModel;

namespace Board2Make
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        //private void OpenInput(object sender, RoutedEventArgs e)
        //{
        //    var dc = DataContext as ViewModel.ViewModel;
                 

        //    var dlg = new OpenFileDialog();
        //    if (File.Exists(dc.boardTxt_filename))
        //    {
        //        dlg.FileName = Path.GetFileName(dc.boardTxt_filename);
        //        dlg.InitialDirectory = Path.GetDirectoryName(dc.boardTxt_filename);
        //    }
        //    if (dlg.ShowDialog() != false)
        //    {
        //        dc.boardTxt_filename = dlg.FileName;
        //    }
        //}


        private void OpenOutput(object sender, RoutedEventArgs e)
        {
            //var dc = DataContext as ViewModel.ViewModel;

            //var dlg = new SaveFileDialog();
            //if (File.Exists(dc.outputFilename))
            //{
            //    dlg.FileName = Path.GetFileName(dc.outputFilename);
            //    dlg.InitialDirectory = Path.GetDirectoryName(dc.outputFilename); 
            //}
            //if (dlg.ShowDialog() != false)
            //{
            //    dc.outputFilename = dlg.FileName;
            //    dc.cmdSave.Execute(null);
            //}

            var dlg = new SaveProjectWin();
            dlg.ShowDialog();

        }
    }
}
