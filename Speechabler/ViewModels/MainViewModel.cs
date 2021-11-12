using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Speechabler.Models;
using Speechabler.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Speechabler.ViewModels
{
    [ViewModel]
    class MainViewModel : NotifyPropertyChangeObject
    {
        public MainViewModel(MessagesViewModel messages, ManualInputMessageViewModel manualInputMessage, SmsReceiversViewModel smsReceivers, SpeechUtil speechUtil)
        {
            Messages = messages;
            ManualInputMessage = manualInputMessage;
            SmsReceivers = smsReceivers;
            SpeechUtil = speechUtil;

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

        private SpeechSynthesizer speechSynthesizer = null;

        public MessagesViewModel Messages { get; }
        public ManualInputMessageViewModel ManualInputMessage { get; }
        public SmsReceiversViewModel SmsReceivers { get; }
        public SpeechUtil SpeechUtil { get; }

        public bool IsEditMode { get => Get(false); private set => Set(value); }

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
