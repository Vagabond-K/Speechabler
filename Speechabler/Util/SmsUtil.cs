using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Speechabler.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Speechabler.Util
{
    [ServiceDescription(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    class SmsUtil
    {
        public SmsUtil(SmsReceiversViewModel smsReceivers)
        {
            this.smsReceivers = smsReceivers;
        }

        private readonly SmsReceiversViewModel smsReceivers;


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
        private string MakeSignature(HttpMethod method, string url, string secretKey, string timeStamp, string accessKeyID)
        {
            using (HMACSHA256 hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                return Convert.ToBase64String(hmacsha256.ComputeHash(
                    Encoding.UTF8.GetBytes($"{method.Method} {url}\n{timeStamp}\n{accessKeyID}")));
            }
        }


        public async Task SendSMS(string message)
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
    }
}
