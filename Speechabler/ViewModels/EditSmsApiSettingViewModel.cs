using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
