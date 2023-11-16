using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dialog.IOMonitor.Models.IOData
{
    public class SignalData : BindableBase
    {
        private bool _signal;
        private string _signalTime;
        private bool _selected = false;

        public bool Signal { get => _signal; set => SetProperty(ref _signal, value); }
        public string SignalTime { get => _signalTime; set => SetProperty(ref _signalTime, value); }
        public bool Selected { get => _selected; set => SetProperty(ref _selected, value); }
    }
}
