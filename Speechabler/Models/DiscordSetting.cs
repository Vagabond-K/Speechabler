using System;

namespace Speechabler.Models
{
    class DiscordSetting : NotifyPropertyChangeObject
    {
        public string WebhookUrl { get => Get<string>(); set => Set(value); }
    }
}
