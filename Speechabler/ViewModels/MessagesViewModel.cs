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
using VagabondK.App;

namespace Speechabler.ViewModels
{
    [ViewModel]
    class MessagesViewModel : NotifyPropertyChangeObject
    {
        public MessagesViewModel(IDialog dialog)
        {
            this.dialog = dialog;
        }

        private readonly IDialog dialog;

        public bool UseMessageButton { get => Get(false); set => Set(value); }

        public MessagesSetting Settings { get => Get(() => new MessagesSetting()); private set => Set(value); }

        public IInstantCommand SetMatrixCommand => GetCommand(async () =>
        {
            UseMessageButton = true;

            if (await dialog.ShowDialog<SetMatrixViewModel, SetMatrixView>("메시지 격자 설정", initViewModel =>
            {
                initViewModel.Rows = Settings.Rows;
                initViewModel.MinRows = 1;
                initViewModel.MaxRows = 6;
                initViewModel.Columns = Settings.Columns;
                initViewModel.MinColumns = 1;
                initViewModel.MaxColumns = 6;
            }, out var viewModel) == true)
                Settings.SetMatrix(viewModel.Rows, viewModel.Columns);
        });

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
