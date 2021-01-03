using Speechabler.Models;
using Speechabler.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Speechabler.ViewModels
{
    class MessagesViewModel : NotifyPropertyChangeObject
    {
        public MessagesSetting Settings { get => Get(() => new MessagesSetting()); private set => Set(value); }

        public IInstantCommand SetMatrixCommand { get => Get(() =>
        {
            var viewModel = new SetMatrixViewModel
            {
                Rows = Settings.Rows,
                MinRows = 1,
                MaxRows = 6,
                Columns = Settings.Columns,
                MinColumns = 1,
                MaxColumns = 6,
            };

            if (new SetMatrixWindow { DataContext = viewModel, Owner = Application.Current.MainWindow }.ShowDialog() == true)
                Settings.SetMatrix(viewModel.Rows, viewModel.Columns);
        }); }

        public void LoadSettings()
        {
            Settings = JsonSetting.LoadSetting<MessagesSetting>() ?? Settings;
        }

        public void SaveSettings()
        {
            JsonSetting.SaveSetting(Settings);
        }
    }
}
