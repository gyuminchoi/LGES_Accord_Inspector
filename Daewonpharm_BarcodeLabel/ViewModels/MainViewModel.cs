using BarcodeLabel.Core.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using PrismTemplate.Services.TimeServices;
using Service.Camera.Models;
using Service.ConnectionCheck.Services;
using Service.Database.Services;
using Service.Logger.Services;
using Service.Postprocessing.Services;
using Service.VisionPro.Services;
using Services.ImageMerge.Services;
using System;
using System.ComponentModel;
using System.Windows;

namespace BarcodeLabel.Main.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private LogWrite _logWrite = LogWrite.Instance;
        private WindowState _windowState;
        private SystemTimeService _timeService = new SystemTimeService();
        private IEventAggregator _eventAggregator;
        private ICameraManager _camManager;
        private IVisionProManager _vpManager;
        private IImageMergeManager _imManager;
        private IPostprocessingManager _postprocessingManager;
        private ISQLiteManager _sqlLiteManager;
        private IConnectionCheckManager _ccManager;
        private string _version;

        public WindowState WindowState { get => _windowState; set => SetProperty(ref _windowState, value); }
        public SystemTimeService TimeService { get => _timeService; set => SetProperty(ref _timeService, value); }
        public string Version { get => _version; set => SetProperty(ref _version, value); }

        public DelegateCommand LoadedCommand => new DelegateCommand(OnLoaded);
        public DelegateCommand<CancelEventArgs> ClosingCommand => new DelegateCommand<CancelEventArgs>(OnClosing);
        public DelegateCommand BtnMinimizeCommand => new DelegateCommand(OnMinimize);

        public MainViewModel(IEventAggregator ea, ICameraManager cm, IVisionProManager vm, IImageMergeManager imManager, IPostprocessingManager postprocessingManager, ISQLiteManager sqliteManager, IConnectionCheckManager ccm) 
        {
            _eventAggregator = ea;
            _camManager = cm;
            _vpManager = vm;
            _imManager = imManager;
            _postprocessingManager = postprocessingManager;
            _ccManager = ccm;
            _sqlLiteManager = sqliteManager;
        }

        private void OnLoaded()
        {
            Version = "Demo";
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
                    _camManager.Closes();
                    _vpManager.Dispose();
                    _imManager.Dispose();
                    _postprocessingManager.Dispose();
                    _ccManager.Dispose();
                    _sqlLiteManager.Dispose();
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
