using BarcodeLabel.Core.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Service.Camera.Models;
using Service.IO.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UI.ConnectState.ViewModels
{
    public class ConnectStateViewModel : BindableBase, IDisposable
    {
        private IDialogService _dialogService;
        private IEventAggregator _eventAggregator;
        private ICameraManager _cameraManager;
        private ServicesInitCompleteEvent _servicesInitCompleteEvent;
        private AppClosingEvent _appClosingEvent;
        private Thread _stateCheckThread = new Thread(() => { });
        private bool _isStateCheck = false;
        private bool _isCameraState = false;

        public bool IsCameraState { get => _isCameraState; set => SetProperty(ref _isCameraState, value); } 

        public DelegateCommand BtnCameraCommand => new DelegateCommand(OnShowCameraStateDialog);
        public DelegateCommand BtnIOStateCommand => new DelegateCommand(OnShowIOStateDialog);
        public DelegateCommand BtnSensorCommand => new DelegateCommand(OnShowSensorStateDialog);
        public DelegateCommand BtnVisionProCommand => new DelegateCommand(OnShowVisionProDialog);


        public ConnectStateViewModel(IDialogService ds, IEventAggregator ea, ICameraManager cm)
        {
            _dialogService = ds;
            _eventAggregator = ea;
            _cameraManager = cm;

            _servicesInitCompleteEvent = _eventAggregator.GetEvent<ServicesInitCompleteEvent>();
            _servicesInitCompleteEvent.Subscribe(OnStateCheckStart);

            _appClosingEvent = _eventAggregator.GetEvent<AppClosingEvent>();
            _appClosingEvent.Subscribe(OnClosing);

        }

        private void OnClosing() => Dispose();

        private void OnStateCheckStart()
        {
            _isStateCheck = true;

            _stateCheckThread = new Thread(new ThreadStart(StateCheckProcess));
            _stateCheckThread.Name = "LiveCam Thread";
            _stateCheckThread.Start();
        }

        private void StateCheckProcess()
        {
            while (_isStateCheck)
            {
                List<bool> camStates = _cameraManager.OpenChecks();
                if(camStates.Any(flag => flag == false)) IsCameraState = false;
                else IsCameraState = true;

                Thread.Sleep(1000);
            }
        }

        private void StateCheckStop()
        {
            _isStateCheck = false;

            if (_stateCheckThread.IsAlive)
            {
                if (_stateCheckThread.Join(500))
                    _stateCheckThread.Abort();
            }
        }

        private void OnShowCameraStateDialog()
        {
            _dialogService.Show("CameraStateDialog");
        }
        private void OnShowIOStateDialog()
        {
            _dialogService.Show("IOMonitorDialog");
        }
        private void OnShowSensorStateDialog()
        {
        }
        private void OnShowVisionProDialog()
        {
        }

        public void Dispose()
        {
            StateCheckStop();
            //TODO : 바꿔야함
            IOManager.Instance.Dispose();
        }
    }
}
