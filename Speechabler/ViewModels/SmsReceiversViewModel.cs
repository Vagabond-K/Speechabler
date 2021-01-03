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
    class SmsReceiversViewModel : NotifyPropertyChangeObject
    {
        public SmsSetting Settings { get => Get(() => new SmsSetting()); private set => Set(value); }

        public IInstantCommand AddReceiverCommand { get => Get(() =>
        {
            SmsReceiver smsReceiver = new SmsReceiver();

            if (EditReceiver(smsReceiver))
                Settings.Receivers.Insert(Settings.Receivers.Count - 1, smsReceiver);
        }); }

        public IInstantCommand EditReceiverCommand { get => Get<SmsReceiver>(smsReceiver => EditReceiver(smsReceiver)); }

        public IInstantCommand RemoveReceiverCommand { get => Get<SmsReceiver>(smsReceiver => Settings.Receivers.Remove(smsReceiver)); }

        private bool EditReceiver(SmsReceiver smsReceiver)
        {
            var viewModel = new EditReceiverViewModel
            {
                Name = smsReceiver.Name,
                PhoneNumber = smsReceiver.PhoneNumber
            };

            if (new EditReceiverWindow { DataContext = viewModel, Owner = Application.Current.MainWindow }.ShowDialog() == true)
            {
                smsReceiver.Name = viewModel.Name;
                smsReceiver.PhoneNumber = viewModel.PhoneNumber;

                return true;
            }

            return false;
        }

        public IInstantCommand EditSmsApiSettingCommand { get => Get(() =>
        {
            var viewModel = new EditSmsApiSettingViewModel
            {
                ServiceID = Settings.SmsApiSetting.ServiceID,
                AccessKeyID = Settings.SmsApiSetting.AccessKeyID,
                SecretKey = Settings.SmsApiSetting.SecretKey,
                SenderPhoneNumber = Settings.SmsApiSetting.SenderPhoneNumber,
            };

            if (new EditSmsApiSettingWindow { DataContext = viewModel, Owner = Application.Current.MainWindow }.ShowDialog() == true)
            {
                Settings.SmsApiSetting.ServiceID = viewModel.ServiceID;
                Settings.SmsApiSetting.AccessKeyID = viewModel.AccessKeyID;
                Settings.SmsApiSetting.SecretKey = viewModel.SecretKey;
                Settings.SmsApiSetting.SenderPhoneNumber = viewModel.SenderPhoneNumber;
            }
        }); }

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
