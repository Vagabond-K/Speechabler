using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Speechabler.Util
{
    [ServiceDescription(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    class DiscordUtil
    {
        public async Task SendWebhook(string message)
        {
            var webhookUrl = "https://discord.com/api/webhooks/여기에_당신의_웹훅_URL";

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
    }
}
