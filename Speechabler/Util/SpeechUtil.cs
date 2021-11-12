using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Speechabler.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace Speechabler.Util
{
    [ServiceDescription(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    class SpeechUtil : NotifyPropertyChangeObject
    {
        public SpeechUtil(SmsReceiversViewModel smsReceivers)
        {
            this.smsReceivers = smsReceivers;
            if (!DesignerProperties.GetIsInDesignMode(new System.Windows.FrameworkElement()))
            {
                speechSynthesizer = new SpeechSynthesizer();
                speechSynthesizer.Volume = 100;
                speechSynthesizer.SetOutputToDefaultAudioDevice();
                speechSynthesizer.SpeakStarted += OnSpeakStarted;
                speechSynthesizer.SpeakCompleted += OnSpeakCompleted;
                visualSpeechSynthesizer = new SpeechSynthesizer();
            }
        }

        private void OnSpeakStarted(object sender, SpeakStartedEventArgs e)
        {
            _ = Task.Run(() =>
            {
                using (var audioStream = new MemoryStream())
                {
                    visualSpeechSynthesizer.SetOutputToAudioStream(audioStream, new SpeechAudioFormatInfo(100, AudioBitsPerSample.Sixteen, AudioChannel.Mono));
                    visualSpeechSynthesizer.Speak(speechText);
                    audioData = audioStream.ToArray();
                }


                if (audioData == null) return;

                var stopwatch = Stopwatch.StartNew();
                int lastIndex = 0;
                double lastValue = 1;
                while (IsSpeeching)
                {
                    int index = (int)stopwatch.ElapsedMilliseconds / 10;
                    if (audioData.Length <= index * 2 + 1)
                    {
                        break;
                    }

                    lastValue -= 0.015 * (index - lastIndex);

                    double value = (double)(audioData[index * 2] << 8 | audioData[index * 2 + 1]) / ushort.MaxValue;
                    if (lastValue < value)
                        lastValue = value;

                    lastIndex = index;

                    AudioVisual = lastValue;
                }
            });
        }

        private void OnSpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            IsSpeeching = false;
        }


        private readonly SpeechSynthesizer speechSynthesizer;
        private readonly SpeechSynthesizer visualSpeechSynthesizer;
        private readonly SmsReceiversViewModel smsReceivers;
        private byte[] audioData;
        private string speechText;
        private string message;

        public bool IsSpeeching { get => Get(false); private set => Set(value); }
        public double AudioVisual { get => Get(0d); private set => Set(value); }

        public IInstantCommand CancelCommand => GetCommand(Cancel);

        public void Cancel()
        {
            speechSynthesizer.SpeakAsyncCancelAll();
            IsSpeeching = false;
        }

        public void Speech(string message)
        {
            Cancel();

            if (!string.IsNullOrWhiteSpace(message))
            {
                try
                {
                    speechText = message.Replace("\n", " ");
                    this.message = message;

                    AudioVisual = 0;
                    IsSpeeching = true;
                    speechSynthesizer.SpeakAsync(speechText);
                }
                catch { }
            }
        }

        private async Task SendSMS()
        {
            try
            {
                var serviceID = smsReceivers.Settings.SmsApiSetting.ServiceID;
                var accessKeyID = smsReceivers.Settings.SmsApiSetting.AccessKeyID;
                var secretKey = smsReceivers.Settings.SmsApiSetting.SecretKey;

                var senderPhoneNumber = new string(smsReceivers.Settings.SmsApiSetting.SenderPhoneNumber.Where(c => char.IsDigit(c)).ToArray());
                var receiverPhoneNumbers = smsReceivers.Settings.Receivers
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
    }
}
