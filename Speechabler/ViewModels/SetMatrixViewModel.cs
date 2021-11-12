using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speechabler.ViewModels
{
    [ViewModel]
    class SetMatrixViewModel : NotifyPropertyChangeObject
    {
        public int Rows { get => Get(3); set => Set(value); }
        public int MinRows { get => Get(1); set => Set(value); }
        public int MaxRows { get => Get(6); set => Set(value); }

        public int Columns { get => Get(3); set => Set(value); }
        public int MinColumns { get => Get(1); set => Set(value); }
        public int MaxColumns { get => Get(6); set => Set(value); }
    }
}
