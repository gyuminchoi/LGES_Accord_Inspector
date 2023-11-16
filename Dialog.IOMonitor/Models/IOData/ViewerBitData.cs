using Prism.Mvvm;
using Service.IO.Models;

namespace Dialog.IOMonitor.Models.IOData
{
    public class ViewerBitData : BindableBase
    {
        private BitData _bitData;
        private bool _isEnable = true;
        private bool _selected = false;
        public BitData BitData { get => _bitData; set => SetProperty(ref _bitData, value); }
        public bool IsEnable { get => _isEnable; set => SetProperty(ref _isEnable, value); }
        public bool Selected { get => _selected; set => SetProperty(ref _selected, value); }

        public ViewerBitData() { }
    }
}
