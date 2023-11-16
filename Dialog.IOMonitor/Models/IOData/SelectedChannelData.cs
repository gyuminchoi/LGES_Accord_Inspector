using Prism.Mvvm;
using Service.IO.Models;

namespace Dialog.IOMonitor.Models.IOData
{
    public class SelectedChannelData : BindableBase
    {
        private IOSlot _ioSlot;
        private BitData _channel;

        public IOSlot IO { get => _ioSlot; set => SetProperty(ref _ioSlot, value); }
        public BitData Channel { get => _channel; set => SetProperty(ref _channel, value); }

        public SelectedChannelData() { }
    }
}


//  (PLC Ready 신호 on -> off) X - 코드 바꿔야함.
//  Reset 시퀀스 동작하기. O - 코드 안바꿔도됨.