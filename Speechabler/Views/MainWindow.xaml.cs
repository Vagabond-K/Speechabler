using Speechabler.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Speechabler.Views
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : ThemeWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Closed += MainWindow_Closed;

            DataContext = MainViewModel;
            MainViewModel.LoadSettings();
        }

        private MainViewModel MainViewModel { get; } = new MainViewModel();

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            MainViewModel.SaveSettings();
        }
    }
}
