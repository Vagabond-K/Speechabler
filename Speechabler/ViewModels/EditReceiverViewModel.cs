using System;

namespace Speechabler.ViewModels
{
    [ViewModel]
    class EditReceiverViewModel : NotifyPropertyChangeObject
    {
        public string Name { get => Get(""); set => Set(value); }
        public string PhoneNumber { get => Get(""); set => Set(value); }
    }
}
