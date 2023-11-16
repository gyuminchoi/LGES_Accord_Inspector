using Prism.Mvvm;
using Service.IO.Models;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace Dialog.IOMonitor.Models.IOData
{
    public class ViewerIOSlotData : BindableBase
    {
        private object _locker = new object();
        private IOSlot _ioData;

        public IOSlot IOData { get => _ioData; set => SetProperty(ref _ioData, value); }
        public ObservableCollection<ViewerBitData> ChannelCollection { get; set; } = new ObservableCollection<ViewerBitData>();

        public ViewerIOSlotData()
        {
            BindingOperations.EnableCollectionSynchronization(ChannelCollection, _locker);
        }
    }
}
