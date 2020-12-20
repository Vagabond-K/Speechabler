using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speechabler.Models
{
    class MessagesSetting : NotifyPropertyChangeObject
    {
        public MessagesSetting() { }

        [JsonConstructor]
        public MessagesSetting(int columns, int rows, MessageItem[] messages)
        {
            Columns = columns;
            Rows = rows;

            if (messages.Length > columns * rows)
                Messages = messages.Take(columns * rows).ToArray();
            else if (messages.Length < columns * rows)
                Messages = messages.Concat(Enumerable.Range(0, Columns * Rows - messages.Length).Select(i => new MessageItem())).ToArray();
            else
                Messages = messages;
        }

        public int Rows { get => Get(3); private set => Set(value); }
        public int Columns { get => Get(3); private set => Set(value); }

        public MessageItem[] Messages { get => Get(() => Enumerable.Range(0, Columns * Rows).Select(i => new MessageItem()).ToArray()); private set => Set(value); }

        public void SetMatrix(int rows, int columns)
        {
            var messages = Messages.ToList();
            var oldRows = Rows;
            var oldColumns = Columns;

            if (oldRows > rows)
                messages.RemoveRange(rows * oldColumns, (oldRows - rows) * oldColumns);
            else if (oldRows < rows)
                messages.AddRange(Enumerable.Range(0, (rows - oldRows) * oldColumns).Select(i => new MessageItem()));

            if (oldColumns > columns)
                for (int row = rows; row > 0; row--)
                    messages.RemoveRange(row * oldColumns - oldColumns + columns, oldColumns - columns);
            else if (oldColumns < columns)
                for (int row = rows; row > 0; row--)
                    messages.InsertRange(row * oldColumns, Enumerable.Range(0, columns - oldColumns).Select(i => new MessageItem()));

            Columns = columns;
            Rows = rows;
            Messages = messages.ToArray();
        }
    }
}
