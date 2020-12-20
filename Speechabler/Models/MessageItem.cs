using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speechabler.Models
{
    class MessageItem : NotifyPropertyChangeObject
    {
        public string Title { get => Get(""); set => Set(value); }
        public string Message { get => Get(""); set => Set(value); }
    }
}
