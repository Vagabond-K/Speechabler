using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Speechabler.Models;
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
    class MainViewModel : NotifyPropertyChangeObject
    {
        public MainViewModel()
        {
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

        public MessagesViewModel Messages { get => Get(() => new MessagesViewModel()); }
        public SmsReceiversViewModel SmsReceivers { get => Get(() => new SmsReceiversViewModel()); }
        
        public bool IsEditMode { get => Get(false); private set => Set(value); }

        public InstantCommand EditContentsCommand { get => Get(() => new InstantCommand(() =>
        {
            SaveSettings();
            SmsReceivers.Settings.Receivers.Add(NewSmsReceiver.Instance);
            IsEditMode = true;
        })); }

        public InstantCommand CancelEditContentsCommand { get => Get(() => new InstantCommand(() =>
        {
            LoadSettings();
            IsEditMode = false;
        })); }

        public InstantCommand SaveContentsCommand { get => Get(() => new InstantCommand(() =>
        {
            SmsReceivers.Settings.Receivers.Remove(NewSmsReceiver.Instance);
            SaveSettings();
            IsEditMode = false;
        })); }


        public InstantCommand<MessageItem> MessageClickCommand { get => Get(() => new InstantCommand<MessageItem>(async (messageItem) =>
        {
            speechSynthesizer.SpeakAsyncCancelAll();

            var message = messageItem.Message.Replace("\r\n", "\n");

            if (string.IsNullOrWhiteSpace(message))
                message = messageItem.Title.Replace("\r\n", "\n");

            if (!string.IsNullOrWhiteSpace(message))
            {
                try
                {
                    speechSynthesizer.SpeakAsync(message.Replace("\n", " "));

                    var serviceID = SmsReceivers.Settings.SmsApiSetting.ServiceID;
                    var accessKeyID = SmsReceivers.Settings.SmsApiSetting.AccessKeyID;
                    var secretKey = SmsReceivers.Settings.SmsApiSetting.SecretKey;

                    var senderPhoneNumber = new string(SmsReceivers.Settings.SmsApiSetting.SenderPhoneNumber.Where(c => char.IsDigit(c)).ToArray());
                    var receiverPhoneNumbers = SmsReceivers.Settings.Receivers
                        .Where(receiver => receiver.IsReceiver && !string.IsNullOrWhiteSpace(receiver.PhoneNumber))
                        .Select(receiver => new string(receiver.PhoneNumber.Where(c => char.IsDigit(c)).ToArray()))
                        .ToArray();

                    if (!string.IsNullOrWhiteSpace(serviceID)
                        && !string.IsNullOrWhiteSpace(accessKeyID)
                        && !string.IsNullOrWhiteSpace(secretKey)
                        && !string.IsNullOrWhiteSpace(senderPhoneNumber)
                        && receiverPhoneNumbers.Length > 0)
                    {
                        using (HttpClient httpClient = new HttpClient())
                        {
                            var request = new HttpRequestMessage(HttpMethod.Post, $"https://sens.apigw.ntruss.com/sms/v2/services/{serviceID}/messages")
                            {
                                Content = new StringContent(JsonConvert.SerializeObject(new SmsSendRequest
                                {
                                    Type = Encoding.UTF8.GetBytes(message).Length <= 80 ? "SMS" : "LMS",
                                    From = senderPhoneNumber,
                                    Content = message,
                                    Messages = receiverPhoneNumbers.Select(phoneNumber => new SmsMessage
                                    {
                                        To = phoneNumber
                                    })
                                }, new JsonSerializerSettings
                                {
                                    NullValueHandling = NullValueHandling.Ignore,
                                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                                }), Encoding.UTF8)
                            };

                            var timeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

                            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };
                            request.Headers.Add("x-ncp-apigw-timestamp", timeStamp);
                            request.Headers.Add("x-ncp-iam-access-key", accessKeyID);
                            request.Headers.Add("x-ncp-apigw-signature-v2", MakeSignature(HttpMethod.Post, $"/sms/v2/services/{serviceID}/messages", secretKey, timeStamp, accessKeyID));

                            var response = await httpClient.SendAsync(request);

                            using (Stream stream = await response.Content.ReadAsStreamAsync())
                            using (StreamReader streamReader = new StreamReader(stream))
                            using (JsonTextReader reader = new JsonTextReader(streamReader))
                            {
                                var result = JsonSerializer.Create().Deserialize<SmsSendResponse>(reader);
                                if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
                                {
                                    //TODO: 전송 오류에 대한 처리 해야 함.
                                }
                            }
                        }
                    }
                }
                catch { }
            }
        })); }

        public string MakeSignature(HttpMethod method, string url, string secretKey, string timeStamp, string accessKeyID)
        {
            using (HMACSHA256 hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                return Convert.ToBase64String(hmacsha256.ComputeHash(
                    Encoding.UTF8.GetBytes($"{method.Method} {url}\n{timeStamp}\n{accessKeyID}")));
            }
        }

        class SmsSendRequest
        {
            public string Type { get; set; }
            public string ContentType { get; set; }
            public string CountryCode { get; set; }
            public string From { get; set; }
            public string Subject { get; set; }
            public string Content { get; set; }
            public IEnumerable<SmsMessage> Messages { get; set; }
            public IEnumerable<SmsFile> Files { get; set; }
            public string ReserveTime { get; set; }
            public string ReserveTimeZone { get; set; }
            public string ScheduleCode { get; set; }
        }

        class SmsMessage
        {
            public string To { get; set; }
            public string Subject { get; set; }
            public string Content { get; set; }
        }

        class SmsFile
        {
            public string Name { get; set; }
            public string Body { get; set; }
        }

        class SmsSendResponse
        {
            public string RequestId { get; set; }
            public DateTime RequestTime { get; set; }
            public string StatusCode { get; set; }
            public string StatusName { get; set; }
        }


        public InstantCommand ShowKeyboardCommand { get => Get(() => new InstantCommand(() =>
        {
            new Process
            {
                StartInfo = new ProcessStartInfo(@"C:\Program Files\Common Files\Microsoft Shared\ink\TabTip.exe")
                {
                    WindowStyle = ProcessWindowStyle.Maximized
                }
            }.Start();
        })); }

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
