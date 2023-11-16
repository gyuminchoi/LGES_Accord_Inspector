using Dialog.IOMonitor.Models.Events;
using Dialog.IOMonitor.Models.IOData;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Service.IO.Services;
using Service.Logger.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Dialog.IOMonitor.ViewModels
{
    public class ChannelAddViewModel : BindableBase, IDisposable
    {
        private IOManager _ioManager = IOManager.Instance;
        private LogWrite _logWrite = LogWrite.Instance;
        private CompleteAddChannelEvent _completeAddChannelEvent;
        private CompleteDeleteChannelEvent _completeDeleteChannelEvent;
        private IEventAggregator _eventAggregator;
        private object _ioCollectionLock = new object();
        private ViewerIOSlotData _selectedIO;
        private ViewerBitData _selectedChannel;



        /// <summary>
        /// IO Collection
        /// </summary>
        public ObservableCollection<ViewerIOSlotData> IODataCollection { get; set; } = new ObservableCollection<ViewerIOSlotData>();
        /// <summary>
        /// 선택한 IO
        /// </summary>
        public ViewerIOSlotData SelectedIO { get => _selectedIO; set => SetProperty(ref _selectedIO, value); }
        /// <summary>
        /// 선택한 채널
        /// </summary>
        public ViewerBitData SelectedChannel
        {
            get => _selectedChannel;
            set
            {
                if (SetProperty(ref _selectedChannel, value))
                {
                    // 채널 선택 시 상위 Collection도 선택 되도록 함
                    SelectedIO = IODataCollection.FirstOrDefault(io => io.ChannelCollection.Contains(value));
                }
            }
        }

        public DelegateCommand LoadedCommand => new DelegateCommand(OnLoaded);
        public DelegateCommand<object[]> BtnAddClickCommand => new DelegateCommand<object[]>(OnCallBackAddChannel);
        public DelegateCommand<Window> BtnCloseClickCommand => new DelegateCommand<Window>(OnCloseWindow);
        public DelegateCommand ClosingCommand => new DelegateCommand(OnClosing);

        public ChannelAddViewModel(IEventAggregator ea)
        {
            _eventAggregator = ea;

            _completeAddChannelEvent = _eventAggregator.GetEvent<CompleteAddChannelEvent>();
            _completeAddChannelEvent.Subscribe(OnDisable);

            _completeDeleteChannelEvent = _eventAggregator.GetEvent<CompleteDeleteChannelEvent>();
            _completeDeleteChannelEvent.Subscribe(OnEnable);
        }

        public void ChangeSelection(ViewerBitData data)
        {
            if (data == null) return;

            foreach (var ioslot in IODataCollection)
            {
                foreach (var channel in ioslot.ChannelCollection)
                {
                    if (data != channel)
                        channel.Selected = false;
                    else
                        channel.Selected = true;
                }
            }
        }

        /// <summary>
        /// 초기 세팅
        /// </summary>
        private void OnLoaded()
        {
            BindingOperations.EnableCollectionSynchronization(IODataCollection, _ioCollectionLock);
            int index = 0;

            // IO 정보를 Collection에 Add
            foreach (var doSlot in _ioManager.DOutputs)
            {
                IODataCollection.Add(new ViewerIOSlotData() { IOData = doSlot });

                foreach (var channel in doSlot.DataBits)
                {
                    ViewerBitData viewerBitData = new ViewerBitData()
                    {
                        IsEnable = true,
                        BitData = channel
                    };

                    IODataCollection[index].ChannelCollection.Add(viewerBitData);
                }

                index++;
            }

            foreach (var diSlot in _ioManager.DInputs)
            {
                IODataCollection.Add(new ViewerIOSlotData() { IOData = diSlot });

                foreach (var channel in diSlot.DataBits)
                {
                    ViewerBitData viewerBitData = new ViewerBitData()
                    {
                        IsEnable = true,
                        BitData = channel
                    };

                    IODataCollection[index].ChannelCollection.Add(viewerBitData);
                }

                index++;
            }
        }

        /// <summary>
        /// 윈도우 종료
        /// </summary>
        /// <param name="window"></param>
        private void OnCloseWindow(Window window) => window.Close();

        /// <summary>
        /// 윈도우 종료하면 동작
        /// </summary>
        private void OnClosing() => Dispose();

        /// <summary>
        /// 선택한 IO, Channel 정보를 ChannelAddEvent를 구독한 곳에 전달
        /// </summary>
        /// <param name="selectedItems">[0] - Selected Slot [1] - Selected Channel</param>
        private void OnCallBackAddChannel(object[] selectedItems)
        {
            if (selectedItems[0] == null)
            {
                MessageBox.Show("Slot을 다시 선택하여 주세요.");
                return;
            }

            if (selectedItems[1] == null)
            {
                MessageBox.Show("Channel을 다시 선택하여 주세요.");
                return;
            }

            var io = (ViewerIOSlotData)selectedItems[0];
            var viewerBitData = (ViewerBitData)selectedItems[1];

            SelectedChannelData selectedIOData = new SelectedChannelData()
            {
                IO = io.IOData,
                Channel = viewerBitData.BitData
            };

            // 이벤트 전달
            _eventAggregator.GetEvent<ChannelAddEvent>().Publish(selectedIOData);
        }

        /// <summary>
        /// Added된 채널 Enable 처리
        /// </summary>
        /// <param name="data">Added Item</param>
        private void OnDisable(List<SignalGraphData> data)
        {
            try
            {
                foreach (var io in IODataCollection)
                {
                    foreach (var channel in io.ChannelCollection)
                    {
                        channel.IsEnable = true;
                    }
                }

                foreach (var item in data)
                {
                    var findIO = IODataCollection.Single(io => io.IOData.Name == item.SelectedChannel.IO.Name && io.IOData.Index == item.SelectedChannel.IO.Index);
                    var findChannel = findIO.ChannelCollection.Single(ch => ch.BitData.MyIndex == item.SelectedChannel.Channel.MyIndex);

                    findChannel.IsEnable = false;
                }
            }
            catch (Exception err)
            {
                _logWrite?.Error(err);
            }
        }

        /// <summary>
        /// 채널 Delete시 Enable 처리
        /// </summary>
        /// <param name="data"></param>
        private void OnEnable(List<SignalGraphData> data)
        {
            try
            {
                if (data.Count == 0)
                {
                    foreach (var io in IODataCollection)
                    {
                        foreach (var channel in io.ChannelCollection)
                        {
                            channel.IsEnable = true;
                        }
                    }
                    return;
                }

                foreach (var item in data)
                {
                    var findIO = IODataCollection.Single(io => io.IOData.Name == item.SelectedChannel.IO.Name && io.IOData.Index == item.SelectedChannel.IO.Index);
                    var findChannel = findIO.ChannelCollection.Single(ch => ch.BitData.MyIndex == item.SelectedChannel.Channel.MyIndex);

                    findChannel.IsEnable = false;
                }
            }
            catch (Exception err)
            {
                _logWrite?.Error(err);
            }
        }

        /// <summary>
        /// 필드, 프로퍼티 Dispose
        /// </summary>
        public void Dispose()
        {
            foreach (var item in IODataCollection)
            {
                item.ChannelCollection.Clear();
            }
            IODataCollection.Clear();

            _completeAddChannelEvent.Unsubscribe(OnDisable);
            _completeDeleteChannelEvent.Unsubscribe(OnEnable);
        }
    }
}
