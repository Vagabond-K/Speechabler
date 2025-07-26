using System;

namespace Speechabler.ViewModels
{
    [ViewModel]
    class EditDiscordSettingViewModel : NotifyPropertyChangeObject
    {
        public string WebhookUrl { get => Get<string>(); set => Set(value); }
    }
}
