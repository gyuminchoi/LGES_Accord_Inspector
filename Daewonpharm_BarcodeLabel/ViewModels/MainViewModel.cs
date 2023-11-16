using BarcodeLabel.Core.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using PrismTemplate.Services.TimeServices;
using Service.Logger.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BarcodeLabel.Main.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private LogWrite _logWrite = LogWrite.Instance;
        private WindowState _windowState;
        private SystemTimeService _timeService = new SystemTimeService();
        private IEventAggregator _eventAggregator;
        private string _version;

        public WindowState WindowState { get => _windowState; set => SetProperty(ref _windowState, value); }
        public SystemTimeService TimeService { get => _timeService; set => SetProperty(ref _timeService, value); }
        public string Version { get => _version; set => SetProperty(ref _version, value); }

        public DelegateCommand LoadedCommand => new DelegateCommand(OnLoaded);
        public DelegateCommand<CancelEventArgs> ClosingCommand => new DelegateCommand<CancelEventArgs>(OnClosing);
        public DelegateCommand BtnMinimizeCommand => new DelegateCommand(OnMinimize);

        public MainViewModel(IEventAggregator ea) 
        {
            _eventAggregator = ea;
            
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
