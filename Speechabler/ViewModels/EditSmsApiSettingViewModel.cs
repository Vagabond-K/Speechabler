using System;

namespace Speechabler.ViewModels
{
    [ViewModel]
    class EditSmsApiSettingViewModel : NotifyPropertyChangeObject
    {
        public string ServiceID { get => Get(""); set => Set(value); }
        public string AccessKeyID { get => Get(""); set => Set(value); }
        public string SecretKey { get => Get(""); set => Set(value); }
        public string SenderPhoneNumber { get => Get(""); set => Set(value); }
    }
}
