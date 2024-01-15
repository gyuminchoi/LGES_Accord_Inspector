using BarcodeLabel.Core.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Service.Camera.Models;
using Service.ConnectionCheck.Models;
using Service.ConnectionCheck.Services;
using Service.Database.Services;
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
        private IConnectionCheckManager _ccManager;
        private ISQLiteManager _sqliteManager;
        private ServicesInitCompleteEvent _servicesInitCompleteEvent;
        private AppClosingEvent _appClosingEvent;
        private Thread _stateCheckThread = new Thread(() => { });
        private ServiceState _state;
        private bool _isStateCheck = false;
        private bool _isCameraState = false;

        public bool IsCameraState { get => _isCameraState; set => SetProperty(ref _isCameraState, value); } 
        public ServiceState State { get => _state; set => SetProperty(ref _state, value); }
        public DelegateCommand BtnCameraCommand => new DelegateCommand(OnShowCameraStateDialog);
        public DelegateCommand BtnIOStateCommand => new DelegateCommand(OnShowIOStateDialog);
        public DelegateCommand BtnDatabaseCommand => new DelegateCommand(OnShowDatabaseStateDialog);
        public DelegateCommand BtnVisionProCommand => new DelegateCommand(OnShowVisionProDialog);


        public ConnectStateViewModel(IDialogService ds, IEventAggregator ea, ICameraManager cm, IConnectionCheckManager ccm, ISQLiteManager sqliteManager)
        {
            _dialogService = ds;
            _eventAggregator = ea;
            _cameraManager = cm;
            _ccManager = ccm;
            _sqliteManager = sqliteManager;

            _servicesInitCompleteEvent = _eventAggregator.GetEvent<ServicesInitCompleteEvent>();
            _servicesInitCompleteEvent.Subscribe(OnStateCheckStart);

            _appClosingEvent = _eventAggregator.GetEvent<AppClosingEvent>();
            _appClosingEvent.Subscribe(OnClosing);
        }

        private void OnClosing() => Dispose();

        private void OnStateCheckStart()
        {
            State = _ccManager.ServiceStates;
            _ccManager.Start();
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

        private void OnShowDatabaseStateDialog()
        {
            _dialogService.Show("DatabaseStateDialog");
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
