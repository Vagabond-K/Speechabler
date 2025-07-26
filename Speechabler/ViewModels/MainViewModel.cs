using Speechabler.Models;
using Speechabler.Util;
using Speechabler.Views;
using System;
using System.ComponentModel;
using System.Speech.Synthesis;
using VagabondK.App;

namespace Speechabler.ViewModels
{
    [ViewModel]
    class MainViewModel : NotifyPropertyChangeObject
    {
        public MainViewModel(
            MessagesViewModel messages,
            ManualInputMessageViewModel manualInputMessage,
            SmsReceiversViewModel smsReceivers,
            SpeechUtil speechUtil,
            DiscordUtil discordUtil,
            SmsUtil smsUtil,
            IDialog dialog)
        {
            Messages = messages;
            ManualInputMessage = manualInputMessage;
            SmsReceivers = smsReceivers;
            SpeechUtil = speechUtil;
            this.smsUtil = smsUtil;
            this.dialog = dialog;
            this.discordUtil = discordUtil;
            Messages.UseMessageButton = true;

            if (DesignerProperties.GetIsInDesignMode(new System.Windows.FrameworkElement()))
            {
                Messages.Settings.Messages[0].Title = "조금\r\n도와주세요";
                Messages.Settings.Messages[0].Message = "저 좀 도와주세요";


                for (int i = 0; i < 2; i++)
                {
                    SmsReceivers.Settings.Receivers.Add(new SmsReceiver
                    {
                        Name = "김방랑",
                        PhoneNumber = "010-1234-5678"
                    });
                }
            }
            else
            {
                speechSynthesizer = new SpeechSynthesizer();
                speechSynthesizer.SetOutputToDefaultAudioDevice();
            }
        }

        private readonly SpeechSynthesizer speechSynthesizer = null;
        private readonly SmsUtil smsUtil;
        private readonly IDialog dialog;
        private readonly DiscordUtil discordUtil;

        public MessagesViewModel Messages { get; }
        public ManualInputMessageViewModel ManualInputMessage { get; }
        public SmsReceiversViewModel SmsReceivers { get; }
        public SpeechUtil SpeechUtil { get; }

        public bool IsEditMode { get => Get(false); private set => Set(value); }

        public IInstantCommand EditDiscordSettingCommand => GetCommand(async () =>
        {
            if (await dialog.ShowDialog<EditDiscordSettingViewModel, EditDiscordSettingView>("Discord 설정", initViewModel =>
            {
                initViewModel.WebhookUrl = discordUtil.Setting.WebhookUrl;
            }, out var viewModel) == true)
            {
                discordUtil.Setting.WebhookUrl = viewModel.WebhookUrl;
                discordUtil.SaveSettings();
            }
        });

        public IInstantCommand EditContentsCommand => GetCommand(() =>
        {
            SaveSettings();
            SmsReceivers.Settings.Receivers.Add(NewSmsReceiver.Instance);
            Messages.UseMessageButton = true;
            IsEditMode = true;
        });

        public IInstantCommand CancelEditContentsCommand => GetCommand(() =>
        {
            LoadSettings();
            IsEditMode = false;
        });

        public IInstantCommand SaveContentsCommand => GetCommand(() =>
        {
            SmsReceivers.Settings.Receivers.Remove(NewSmsReceiver.Instance);
            SaveSettings();
            IsEditMode = false;
        });


        public IInstantCommand MessageClickCommand => GetCommand<MessageItem>(messageItem =>
        {
            var message = messageItem.Message.Replace("\r\n", "\n");

            if (string.IsNullOrWhiteSpace(message))
                message = messageItem.Title.Replace("\r\n", "\n");

            _ = discordUtil.SendWebhook(message);
            _ = smsUtil.SendSMS(message);
            SpeechUtil.Speech(message);
        });

        public void LoadSettings()
        {
            Messages.LoadSettings();
            SmsReceivers.LoadSettings();
        }

        public void SaveSettings()
        {
            Messages.SaveSettings();
            SmsReceivers.SaveSettings();
        }
    }
}
