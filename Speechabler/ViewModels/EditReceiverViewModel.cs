using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speechabler.ViewModels
{
    [ViewModel]
    class EditReceiverViewModel : NotifyPropertyChangeObject
    {
        public string Name { get => Get(""); set => Set(value); }
        public string PhoneNumber { get => Get(""); set => Set(value); }
    }
}
