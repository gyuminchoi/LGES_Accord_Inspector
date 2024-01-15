using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Setting.Models
{
    public class IOData : BindableBase
    {
        private int _slot;
        private int _index;

        public int Slot { get => _slot; set => SetProperty(ref _slot, value); }
        public int Index { get => _index; set => SetProperty(ref _index, value); }
    }
}
