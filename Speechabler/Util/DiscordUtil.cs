using Newtonsoft.Json;
using Speechabler.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Speechabler.Util
{
    [ServiceDescription(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    class DiscordUtil
    {
        public DiscordUtil()
        {
            LoadSettings();
        }

        public DiscordSetting Setting { get; private set; }

        public async Task SendWebhook(string message)
        {
            var webhookUrl = Setting.WebhookUrl;

            if (string.IsNullOrWhiteSpace(webhookUrl) || string.IsNullOrWhiteSpace(message))
                return;

            var payload = new
            {
                content = message
            };

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync(webhookUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    //TODO: 전송 오류에 대한 처리 해야 함.
                }
            }
        }

        public void LoadSettings()
        {
            Setting = JsonSetting.LoadSetting<DiscordSetting>() ?? new DiscordSetting();
        }

        public void SaveSettings()
        {
            JsonSetting.SaveSetting(Setting);
        }
    }
}
