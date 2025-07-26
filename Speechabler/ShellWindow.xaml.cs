using Microsoft.Extensions.DependencyInjection;
using Speechabler.ViewModels;
using Speechabler.Views;
using System;
using System.Windows;
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
