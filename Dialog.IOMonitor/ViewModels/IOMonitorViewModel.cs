using Dialog.IOMonitor.Models.Events;
using Dialog.IOMonitor.Models.IOData;
using Dialog.IOMonitor.Models.States;
using Dialog.IOMonitor.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Service.IO.Services;
using Service.Logger.Services;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Data;
using System.Windows;
using Service.CustomException.Services.ErrorService.HandledExceptions;
using Service.CustomException.Models.ErrorTypes;

namespace Dialog.IOMonitor.ViewModels
{
    public class IOMonitorViewModel : BindableBase, IDialogAware, IDisposable
    {
        private IOManager _ioManager = IOManager.Instance;
        private LogWrite _logWrite = LogWrite.Instance;
        //private IORecipeRepository _ioSettingRepo = IORecipeRepository.Instance;
        private Thread _signalCheckThread;
        private ChannelAddWindow _channelAddWindow;
        private IOControllerWindow _ioControllerWindow;
        private ChannelAddEvent _channelAddEvnet;
        private ESignalCheckState _signalCheckState;
        private IEventAggregator _eventAggregator;
        //private string _inputRecipeName;
        private string _selectedRecipe;
        private bool _isRun = false;
        private object _channelCollectionLock = new object();
        private string _tackTime;

        public string Title => "IO Monitor";

        public event Action<IDialogResult> RequestClose;

        public string TackTime { get => _tackTime; set => SetProperty(ref _tackTime, value); }
        /// <summary>
        /// SiganlGraph 시작 상태
        /// </summary>
        public ESignalCheckState SignalCheckState { get => _signalCheckState; set => SetProperty(ref _signalCheckState, value); }
        /// <summary>
        /// Channel ListView
        /// </summary>
        public ObservableCollection<SignalGraphData> ChannelCollection { get; set; } = new ObservableCollection<SignalGraphData>();
        /// <summary>
        /// Recipe ComboBox
        /// </summary>
        public ObservableCollection<string> RecipeCollection { get; set; } = new ObservableCollection<string>();
        /// <summary>
        /// 입력한 레시피명
        /// </summary>
        //public string InputRecipeName { get => _inputRecipeName; set => SetProperty(ref _inputRecipeName, value); }
        /// <summary>
        /// 선택한 레시피
        /// </summary>
        public string SelectedRecipe { get => _selectedRecipe; set => SetProperty(ref _selectedRecipe, value); }


        public DelegateCommand LoadedCommand => new DelegateCommand(OnLoaded);
        public DelegateCommand ClosingCommand => new DelegateCommand(OnClosing);
        public DelegateCommand BtnAddChannelCommand => new DelegateCommand(OnShowAddChannelWindow);
        public DelegateCommand<object> ItemClickCommand => new DelegateCommand<object>(OnMarkingVerticalLine);
        public DelegateCommand BtnAllDeleteCommand => new DelegateCommand(OnClearCollection);
        public DelegateCommand BtnStartStopCommand => new DelegateCommand(OnRunStartStop);
        //public DelegateCommand<string> BtnRecipeSaveClickCommand => new DelegateCommand<string>(OnSaveRecipe);
        //public DelegateCommand<string> CdoDropDownClosedCommand => new DelegateCommand<string>(OnApplyRecipe);
        public DelegateCommand<object[]> BtnDeleteChannelCollection => new DelegateCommand<object[]>(OnDeleteChannel);
        //public DelegateCommand<string> BtnDeleteRecipeCommand => new DelegateCommand<string>(OnDeleteRecipe);
        public DelegateCommand BtnShowIOMonitorCommand => new DelegateCommand(OnShowIOMonitor);

        public IOMonitorViewModel(IEventAggregator ea)
        {
            _eventAggregator = ea;
            // 이벤트
            _channelAddEvnet = _eventAggregator.GetEvent<ChannelAddEvent>();
            _channelAddEvnet.Subscribe(OnAddChannel);
        }

        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }

        /// <summary>
        /// 초기 세팅
        /// </summary>
        private void OnLoaded()
        {
            BindingOperations.EnableCollectionSynchronization(ChannelCollection, _channelCollectionLock);

            try
            {
                // App이나 Bootstrappers에서 시작 한다면 할 필요 없음 그게 아니라면 주석해제
                //=====================================================================
                //_ioManager.Open("192.168.100.100");

                // false - IOManager 내부적으로 돌아가는 채널 Read 스레드 사용 안함

                //=====================================================================

                // 레시피 받아오기 (Window 오픈할 때 가져올 거면 주석해제)
                //=====================================================================
                //_ioSettingRepo.Deserialize();
                //InitRecipe();
                //=====================================================================

                SignalCheckState = ESignalCheckState.Stopped;
            }
            catch (Exception ex)
            {
                _logWrite?.Error(ex, true, true);
            }
        }

        /// <summary>
        /// Window Closing될 때 실행
        /// </summary>
        private void OnClosing() => Dispose();


        /// <summary>
        /// All Clear
        /// </summary>
        private void OnClearCollection()
        {
            foreach (var item in ChannelCollection)
            {
                item.SignalRecordCollection.Clear();
            }
            ChannelCollection.Clear();

            List<SignalGraphData> graphs = ChannelCollection.ToList();
            _eventAggregator.GetEvent<CompleteDeleteChannelEvent>().Publish(graphs);
        }

        /// <summary>
        /// Channel 개별 삭제
        /// </summary>
        /// <param name="data">[0] - Channel Collection, [1] - 그래프 데이터</param>
        private void OnDeleteChannel(object[] data)
        {
            ObservableCollection<SignalGraphData> channelCollection = (ObservableCollection<SignalGraphData>)data[0];
            SignalGraphData item = (SignalGraphData)data[1];

            item.SignalRecordCollection.Clear();

            if (!channelCollection.Remove(item))
                MessageBox.Show("다시 시도해 주세요.");

            // 이벤트 전달
            List<SignalGraphData> graphs = ChannelCollection.ToList();
            _eventAggregator.GetEvent<CompleteAddChannelEvent>().Publish(graphs);
        }

        /// <summary>
        /// 그래프 시작 or 종료
        /// </summary>
        private void OnRunStartStop()
        {
            if (ChannelCollection.Count < 1)
            {
                MessageBox.Show("채널을 추가해 주세요.");
                return;
            }

            switch (SignalCheckState)
            {
                case ESignalCheckState.Stopped:
                    Run();
                    break;
                case ESignalCheckState.Running:
                    Stop();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Add Channel 윈도우 열기
        /// </summary>
        private void OnShowAddChannelWindow()
        {
            if (_channelAddWindow != null && _channelAddWindow.IsLoaded)
                return;

            _channelAddWindow = new ChannelAddWindow();
            _channelAddWindow.Show();

            List<SignalGraphData> channels = ChannelCollection.ToList();
            _eventAggregator.GetEvent<CompleteAddChannelEvent>().Publish(channels);
        }

        /// <summary>
        /// ChannelCollection에 채널 추가
        /// </summary>
        /// <param name="data">채널 데이터</param>
        private void OnAddChannel(SelectedChannelData data)
        {
            // 중복 방지
            foreach (var ch in ChannelCollection)
            {
                if (ch.SelectedChannel.IO.Name == data.IO.Name && ch.SelectedChannel.Channel == data.Channel)
                {
                    MessageBox.Show("이미 추가된 채널 입니다.");
                    return;
                }
            }

            // 없다면 Add
            SignalGraphData channel = new SignalGraphData()
            {
                SelectedChannel = data,
            };

            ChannelCollection.Add(channel);

            // 이벤트 전달
            List<SignalGraphData> channels = ChannelCollection.ToList();
            _eventAggregator.GetEvent<CompleteAddChannelEvent>().Publish(channels);
        }

        /// <summary>
        /// Signal Collection에 마킹
        /// </summary>
        /// <param name="selectedIndex">선택한 신호 인덱스</param>
        private void OnMarkingVerticalLine(object selectedIndex)
        {
            int index = (int)selectedIndex;

            if (index < 0)
                return;

            foreach (var channel in ChannelCollection)
            {
                if (channel.SignalRecordCollection.Count >= index + 1)
                {
                    if (!channel.SignalRecordCollection[index].Selected)
                        channel.SignalRecordCollection[index].Selected = true;      // 마킹
                    else
                        channel.SignalRecordCollection[index].Selected = false;     // 마킹 지우기
                }
            }
        }

        /// <summary>
        /// 시그널 그래프 업데이트 시작
        /// </summary>
        private void Run()
        {
            try
            {
                if (_signalCheckThread != null)
                {
                    _signalCheckThread.Join(1000);
                    _signalCheckThread = null;
                }

                foreach (var item in ChannelCollection)
                {
                    item.SignalRecordCollection.Clear();
                }

                _isRun = true;

                _signalCheckThread = new Thread(new ThreadStart(SignalCheckThread));
                _signalCheckThread.Name = "SignalCheckThread";
                _signalCheckThread.Start();

                SignalCheckState = ESignalCheckState.Running;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// 그래프 업데이트 스레드 종료
        /// </summary>
        private void Stop()
        {
            if (_signalCheckThread != null)
            {
                _isRun = false;

                if (!_signalCheckThread.Join(1000))
                    _signalCheckThread.Abort();

                _signalCheckThread = null;

                SignalCheckState = ESignalCheckState.Stopped;
            }
        }

        /// <summary>
        /// 그래프 업데이트
        /// </summary>
        /// <exception cref="FnIOException"></exception>
        private void SignalCheckThread()
        {
            int errCount = 0;
            while (_isRun)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                try
                {
                    // Signal Add
                    AddSignalCollection();

                    errCount = 0;
                    Thread.Sleep(30);
                    stopwatch.Stop();
                    TackTime = stopwatch.Elapsed.ToString();
                }
                catch (ThreadAbortException) { throw; }
                catch (Exception ex)
                {
                    _logWrite.Error(ex, false, true); // 로그파일에만 남김.
                    if (errCount > 100) { throw new FnIOException(null, EFnIOError.FnIOSignalCheckThreadDie); }
                    errCount++;
                }
            }
        }

        /// <summary>
        /// 그래프 업데이트 로직
        /// </summary>
        private void AddSignalCollection()
        {
            foreach (var channel in ChannelCollection)
            {
                // Digital Input
                var diSlot = _ioManager.DInputs.SingleOrDefault(item => item.Name == channel.SelectedChannel.IO.Name && item.Index == channel.SelectedChannel.IO.Index);
                if (diSlot != null)
                {
                    // 로직 처음 시작 시 
                    if (channel.SignalRecordCollection.Count == 0)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            channel.SignalRecordCollection.Add(new SignalData()
                            {
                                Signal = diSlot.DataBits[channel.SelectedChannel.Channel.MyIndex].Value,
                                SignalTime = DateTime.Now.ToString("mm:ss")
                            });
                        });

                        continue;
                    }

                    // 이전 신호와 다르다면
                    if (channel.SignalRecordCollection[channel.SignalRecordCollection.Count - 1].Signal != diSlot.DataBits[channel.SelectedChannel.Channel.MyIndex].Value)
                    {
                        // 바뀐 신호 Add
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            channel.SignalRecordCollection.Add(new SignalData()
                            {
                                Signal = diSlot.DataBits[channel.SelectedChannel.Channel.MyIndex].Value,
                                SignalTime = DateTime.Now.ToString("mm:ss")
                            });
                        });

                        // 다른 채널 직전 신호 Add
                        foreach (var item in ChannelCollection)
                        {
                            if (item != channel)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    item.SignalRecordCollection.Add(new SignalData()
                                    {
                                        Signal = item.SignalRecordCollection[item.SignalRecordCollection.Count - 1].Signal
                                    });
                                });
                            }
                        }

                        continue;
                    }
                }

                // Digital Output
                var doSlot = _ioManager.DOutputs.SingleOrDefault(item => item.Name == channel.SelectedChannel.IO.Name && item.Index == channel.SelectedChannel.IO.Index);
                if (doSlot != null)
                {
                    // 로직 처음 시작 시 
                    if (channel.SignalRecordCollection.Count == 0)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            channel.SignalRecordCollection.Add(new SignalData()
                            {
                                Signal = doSlot.DataBits[channel.SelectedChannel.Channel.MyIndex].Value,
                                SignalTime = DateTime.Now.ToString("mm:ss")
                            });
                        });

                        continue;
                    }

                    // 이전 신호와 다르다면
                    if (channel.SignalRecordCollection[channel.SignalRecordCollection.Count - 1].Signal != doSlot.DataBits[channel.SelectedChannel.Channel.MyIndex].Value)
                    {
                        // 바뀐 신호 Add
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            channel.SignalRecordCollection.Add(new SignalData()
                            {
                                Signal = doSlot.DataBits[channel.SelectedChannel.Channel.MyIndex].Value,
                                SignalTime = DateTime.Now.ToString("mm:ss")
                            });
                        });

                        // 다른 채널 직전 신호 Add
                        foreach (var item in ChannelCollection)
                        {
                            if (item != channel)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    item.SignalRecordCollection.Add(new SignalData()
                                    {
                                        Signal = item.SignalRecordCollection[item.SignalRecordCollection.Count - 1].Signal
                                    });
                                });
                            }
                        }
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// 레시피 사용
        /// </summary>
        /// <param name="recipe"></param>
        //private void OnApplyRecipe(string recipe)
        //{
        //    // null 검사
        //    if (String.IsNullOrEmpty(recipe))
        //        return;

        //    // IO가 있는지, 인덱스가 바뀌진 않았는 지 검사
        //    // 사용할 수 없다면 레시피 삭제 후 Json 업데이트
        //    foreach (var item in _ioSettingRepo.RecipeDictionary[recipe])
        //    {

        //        bool isCheck = IOChangePointCheck(item);
        //        if (!isCheck)
        //        {
        //            MessageBox.Show("사용할 수 없는 레시피 입니다.");
        //            _ioSettingRepo.RecipeDictionary.Remove(recipe);

        //            List<string> keys = _ioSettingRepo.SerializeUpdate();
        //            RecipeCollection.Clear();
        //            foreach (var key in keys)
        //            {
        //                RecipeCollection.Add(key);
        //            }
        //            return;
        //        }
        //    }

        //    // 채널 Add
        //    ChannelCollection.Clear();
        //    List<SignalGraphData> channels = _ioSettingRepo.RecipeDictionary[recipe];
        //    foreach (var channel in channels)
        //    {
        //        channel.SignalRecordCollection.Clear();
        //        ChannelCollection.Add(channel);
        //    }

        //    // 이벤트 전달
        //    List<SignalGraphData> graphs = ChannelCollection.ToList();
        //    _eventAggregator.GetEvent<CompleteAddChannelEvent>().Publish(graphs);

        //    // UI 변경
        //    InputRecipeName = string.Empty;
        //}

        /// <summary>
        /// IO 존재 여부, 실제 IO 정보와 같은 지 검사
        /// </summary>
        /// <param name="channel">레시피에 저장된 데이터</param>
        private bool IOChangePointCheck(SignalGraphData channel)
        {
            if (channel.SelectedChannel.IO.Name.Substring(channel.SelectedChannel.IO.Name.IndexOf('-'),1) == "1")
            {
                bool isAny = _ioManager.DOutputs.Any(slot => slot.Name == channel.SelectedChannel.IO.Name && slot.Index == channel.SelectedChannel.IO.Index);

                if (!isAny)
                    return false;
            }

            if (channel.SelectedChannel.IO.Name.Substring(channel.SelectedChannel.IO.Name.IndexOf('-'), 1) == "2")
            {
                bool isAny = _ioManager.DInputs.Any(slot => slot.Name == channel.SelectedChannel.IO.Name && slot.Index == channel.SelectedChannel.IO.Index);

                if (!isAny)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 레시피 저장
        /// </summary>
        /// <param name="recipeName">TextBox 입력값</param>
        //private void OnSaveRecipe(string recipeName)
        //{
        //    // 중복검사
        //    foreach (var item in RecipeCollection)
        //    {
        //        if (item == recipeName)
        //        {
        //            MessageBox.Show("이미 존재하는 레시피 이름 입니다.");
        //            return;
        //        }
        //    }

        //    // Value 생성 및 신호 Clear
        //    List<SignalGraphData> sinalGraphList = new List<SignalGraphData>(ChannelCollection);
        //    foreach (var item in sinalGraphList)
        //    {
        //        item.SignalRecordCollection.Clear();
        //    }

        //    // KeyValue 생성 및 Serialize
        //    KeyValuePair<string, List<SignalGraphData>> recipe = new KeyValuePair<string, List<SignalGraphData>>(recipeName, sinalGraphList);

        //    _ioSettingRepo.Serialize(recipe);

        //    // ComboBox Add
        //    RecipeCollection.Add(recipeName);

        //    // UI 변경
        //    InputRecipeName = string.Empty;
        //    SelectedRecipe = recipeName;
        //}

        /// <summary>
        /// RecipeDictionary에서 데이터 받아옴
        /// </summary>
        //private void InitRecipe()
        //{
        //    if (_ioSettingRepo.RecipeDictionary == null)
        //        return;

        //    foreach (var item in _ioSettingRepo.RecipeDictionary)
        //    {
        //        RecipeCollection.Add(item.Key);
        //    }
        //}

        private void OnShowIOMonitor()
        {
            if (_ioControllerWindow != null && _ioControllerWindow.IsLoaded)
                return;

            _ioControllerWindow = new IOControllerWindow();
            _ioControllerWindow.Show();
        }

        public void Dispose()
        {
            if (_channelAddWindow != null && _channelAddWindow.IsLoaded)
                _channelAddWindow.Close();

            //if (_ioControllerWindow != null && _ioControllerWindow.IsLoaded)
            //    _ioControllerWindow.Close();

            try { Stop(); } catch (Exception ex) { _logWrite.Error(ex, false, true); }

            foreach (var channelData in ChannelCollection)
            {
                channelData.SignalRecordCollection.Clear();
            }
            ChannelCollection.Clear();

            _channelAddEvnet.Unsubscribe(OnAddChannel);
        }
    }
}
