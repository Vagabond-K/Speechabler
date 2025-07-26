using System;
using System.Collections.ObjectModel;

namespace Speechabler.Models
{
    class SmsSetting : NotifyPropertyChangeObject
    {
        public SmsApiSetting SmsApiSetting { get => Get(() => new SmsApiSetting()); set => Set(value); }

        public ObservableCollection<SmsReceiver> Receivers { get; } = new ObservableCollection<SmsReceiver>();
    }
}
