using CREVIS_SWIR_Inspector.Core.Events;
using CREVIS_SWIR_Inspector.Services.TimeServices;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Service.Camera.Models;
using Service.Logger.Services;
using Service.MainInspection.Services;
using Service.Postprocessing.Services;
using Service.VisionPro.Services;
using System;
using System.ComponentModel;
using System.Windows;

namespace CREVIS_SWIR_Inspector.Main.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private LogWrite _logWrite = LogWrite.Instance;
        private ICameraManager _camManager;
        private IVisionProManager _vpManager;
        private IPostprocessingManager _postprocessingManager;
        private IMainInpectionManager _miManager;
        private IEventAggregator _eventAggregator;
        private SystemTimeService _timeService = new SystemTimeService();
        private WindowState _windowState;
        private string _version;

        public WindowState WindowState { get => _windowState; set => SetProperty(ref _windowState, value); }
        public SystemTimeService TimeService { get => _timeService; set => SetProperty(ref _timeService, value); }
        public string Version { get => _version; set => SetProperty(ref _version, value); }

        public DelegateCommand LoadedCommand => new DelegateCommand(OnLoaded);
        public DelegateCommand<CancelEventArgs> ClosingCommand => new DelegateCommand<CancelEventArgs>(OnClosing);
        public DelegateCommand BtnMinimizeCommand => new DelegateCommand(OnMinimize);

        public MainViewModel(IEventAggregator ea, ICameraManager cm, IVisionProManager vm, IPostprocessingManager ppm, IMainInpectionManager mim) 
        {
            _eventAggregator = ea;
            _camManager = cm;
            _vpManager = vm;
            _postprocessingManager = ppm;
            _miManager = mim;
        }

        private void OnLoaded()
        {
            Version = "Ver 1.0.0";
            WindowState = WindowState.Maximized;
            TimeService.DispatcherTimerStart();
        }

        private void OnClosing(CancelEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit?", "Exit", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                // TODO : Dispose 구현해야함
                _logWrite?.Info("Application Closing...");

                try
                {
                    _miManager.Dispose();
                    _camManager.Closes();
                    _vpManager.Dispose();
                    _postprocessingManager.Dispose();
                }
                catch 
                {
                    Environment.Exit(0);
                    Application.Current.Shutdown();
                }
                _eventAggregator.GetEvent<AppClosingEvent>().Publish();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void OnMinimize()
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Minimized;
        }
    }
}
