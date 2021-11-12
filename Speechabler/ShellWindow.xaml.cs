using Microsoft.Extensions.DependencyInjection;
using Speechabler.ViewModels;
using Speechabler.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VagabondK.App;
using VagabondK.App.Windows;

namespace Speechabler
{
    public partial class ShellWindow : VagabondK.Windows.ThemeWindow
    {
        public ShellWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;

            var services = new ServiceCollection();
            services.AddServices(typeof(Dialog).Assembly);
            services.AddServices(typeof(ShellWindow).Assembly);
            shell = new SimpleShell(services);
            DataContext = shell;
        }


        private readonly Shell shell;

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var pageContext = await shell.OpenPage<MainViewModel, MainView>();
            (pageContext.ViewModel as MainViewModel)?.LoadSettings();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            (shell.SelectedPageContext?.ViewModel as MainViewModel)?.SaveSettings();
            (shell.SelectedPageContext?.ViewModel as MainViewModel)?.SpeechUtil?.Cancel();
        }
    }
}
