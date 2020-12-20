using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speechabler.Models
{
    class SmsSetting : NotifyPropertyChangeObject
    {
        public SmsApiSetting SmsApiSetting { get => Get(() => new SmsApiSetting()); set => Set(value); }

        public ObservableCollection<SmsReceiver> Receivers { get; } = new ObservableCollection<SmsReceiver>();
    }
}
