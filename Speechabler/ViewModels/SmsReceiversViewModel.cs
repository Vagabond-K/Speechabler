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
    class SmsReceiversViewModel : NotifyPropertyChangeObject
    {
        public SmsReceiversViewModel(IDialog dialog)
        {
            this.dialog = dialog;
        }

        private readonly IDialog dialog;

        public SmsSetting Settings { get => Get(() => new SmsSetting()); private set => Set(value); }

        public IInstantCommand AddReceiverCommand => GetCommand(async () =>
        {
            SmsReceiver smsReceiver = new SmsReceiver();

            if (await EditReceiver(smsReceiver))
                Settings.Receivers.Insert(Settings.Receivers.Count - 1, smsReceiver);
        });

        public IInstantCommand EditReceiverCommand => GetCommand<SmsReceiver>(async smsReceiver => await EditReceiver(smsReceiver));

        public IInstantCommand RemoveReceiverCommand => GetCommand<SmsReceiver>(smsReceiver => Settings.Receivers.Remove(smsReceiver));

        private async Task<bool> EditReceiver(SmsReceiver smsReceiver)
        {
            if (await dialog.ShowDialog<EditReceiverViewModel, EditReceiverView>("SMS 수신자 정보 편집", initViewModel =>
            {
                initViewModel.Name = smsReceiver.Name;
                initViewModel.PhoneNumber = smsReceiver.PhoneNumber;
            }, out var viewModel) == true)
            {
                smsReceiver.Name = viewModel.Name;
                smsReceiver.PhoneNumber = viewModel.PhoneNumber;

                return true;
            }

            return false;
        }

        public IInstantCommand EditSmsApiSettingCommand => GetCommand(async () =>
        {
            if (await dialog.ShowDialog<EditSmsApiSettingViewModel, EditSmsApiSettingView>("SMS API 설정", initViewModel =>
            {
                initViewModel.ServiceID = Settings.SmsApiSetting.ServiceID;
                initViewModel.AccessKeyID = Settings.SmsApiSetting.AccessKeyID;
                initViewModel.SecretKey = Settings.SmsApiSetting.SecretKey;
                initViewModel.SenderPhoneNumber = Settings.SmsApiSetting.SenderPhoneNumber;
            }, out var viewModel) == true)
            {
                Settings.SmsApiSetting.ServiceID = viewModel.ServiceID;
                Settings.SmsApiSetting.AccessKeyID = viewModel.AccessKeyID;
                Settings.SmsApiSetting.SecretKey = viewModel.SecretKey;
                Settings.SmsApiSetting.SenderPhoneNumber = viewModel.SenderPhoneNumber;
            }
        });

        public void LoadSettings()
        {
            Settings = JsonSetting.LoadSetting<SmsSetting>() ?? Settings;
        }

        public void SaveSettings()
        {
            JsonSetting.SaveSetting(Settings);
        }
    }

    class NewSmsReceiver : SmsReceiver
    {
        public static SmsReceiver Instance { get; } = new NewSmsReceiver();
    }
}
