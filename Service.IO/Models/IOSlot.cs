using Prism.Mvvm;

namespace Service.IO.Models
{
    public class IOSlot : BindableBase
    {
        #region Fields For Properties
        private string _name;
        private BitData[] _dataBits;
        private int _index;
        private int _size;
        private byte[] _dataBytes;
        #endregion 
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public BitData[] DataBits { get => _dataBits; set => SetProperty(ref _dataBits, value); }
        // Test
        public byte[] DataBytes { get => _dataBytes; set => SetProperty(ref _dataBytes, value); }

        public int Index { get => _index; set => SetProperty(ref _index, value); }
        public int Size { get => _size; set => SetProperty(ref _size, value); }
        public IOSlot()
        {
            Name = "Unknown";
            DataBits = null;
            Index = -1;
            Size = 0;
        }

    }
}
