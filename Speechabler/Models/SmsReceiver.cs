using System;

namespace Speechabler.Models
{
    class SmsReceiver : NotifyPropertyChangeObject
    {
        public bool IsReceiver { get => Get(false); set => Set(value); }
        public string Name { get => Get(""); set => Set(value); }
        public string PhoneNumber { get => Get(""); set => Set(value); }
    }
}
