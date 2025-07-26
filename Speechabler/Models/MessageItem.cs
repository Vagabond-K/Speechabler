using System;

namespace Speechabler.Models
{
    class MessageItem : NotifyPropertyChangeObject
    {
        public string Title { get => Get(""); set => Set(value); }
        public string Message { get => Get(""); set => Set(value); }
    }
}
