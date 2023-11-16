using Prism.Mvvm;
using Service.Pattern;
using System.Windows.Data;

namespace Dialog.IOMonitor.Models.IOData
{
    public class SignalGraphData : BindableBase
    {
        private SelectedChannelData _selectedChannel;
        private object _signalRecordLock = new object();

        public SelectedChannelData SelectedChannel { get => _selectedChannel; set => SetProperty(ref _selectedChannel, value); }

        public AutoDeleteObservableCollection<SignalData> SignalRecordCollection { get; set; } = new AutoDeleteObservableCollection<SignalData>(60);
        
        public SignalGraphData()
        {
            BindingOperations.EnableCollectionSynchronization(SignalRecordCollection, _signalRecordLock);
        }
    }
}
