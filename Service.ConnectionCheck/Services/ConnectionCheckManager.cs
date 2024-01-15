using Prism.Mvvm;
using Service.Camera.Models;
using Service.ConnectionCheck.Models;
using Service.Database.Services;
using Service.IO.Services;
using Service.Logger.Services;
using Service.VisionPro.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Service.ConnectionCheck.Services
{
    public class ConnectionCheckManager : BindableBase, IConnectionCheckManager
    {
        private ServiceState _serviceStates = new ServiceState();
        private Thread _stateCheckThread = new Thread(() => { });
        private IOManager _ioManager = IOManager.Instance;
        private LogWrite _logWrite = LogWrite.Instance;
        private ICameraManager _camManager;
        private IVisionProManager _vpManager;
        private ISQLiteManager _sqliteManager;

        public bool IsRun { get; set; } = false;
        public ServiceState ServiceStates { get => _serviceStates; set => SetProperty(ref _serviceStates, value); }

        public ConnectionCheckManager() { }

        public void Initialize(ICameraManager cm, IVisionProManager vpm, ISQLiteManager sqliteManager)
        {
            _camManager = cm;
            _vpManager = vpm;
            _sqliteManager = sqliteManager;
        }

        public void Start()
        {
            try
            {
                IsRun = true;

                _stateCheckThread = new Thread(new ThreadStart(StateCheckProcess));
                _stateCheckThread.Name = "State Check Thread";
                _stateCheckThread.Start();
            }
            catch (Exception err)
            {
                _logWrite?.Error(err);
                Stop();
                IsRun = false;
            }

        }

        public void Stop()
        {
            try
            {
                if (!_stateCheckThread.IsAlive)
                    return;

                IsRun = false;

                if (_stateCheckThread.Join(500))
                    _stateCheckThread.Abort();
            }
            catch (Exception err)
            {
                _logWrite?.Error(err);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private void StateCheckProcess()
        {
            while (IsRun)
            {
                ServiceStates.CameraState = CheckCameraState();
                ServiceStates.IOState = CheckIOState();
                ServiceStates.DatabaseState = CheckDataBaseState();
                Thread.Sleep(3000);
            }
        }

        private bool CheckCameraState()
        {
            bool result = true;
            List<bool> camStates = _camManager.OpenChecks();

            if (camStates.Any(flag => flag == false)) result = false;
            if (_camManager.CameraDic.Count != 4) result = false;

            return result;
        }

        private bool CheckIOState()
        {
            bool result = _ioManager.CheckInternalBus();
            return result;
        }

        private bool CheckDataBaseState()
        {
            ConnectionState result = _sqliteManager.CheckDBState();
            switch (result)
            {
                case ConnectionState.Open:
                case ConnectionState.Connecting:
                case ConnectionState.Executing:
                case ConnectionState.Fetching:
                    return true;

                case ConnectionState.Closed:
                case ConnectionState.Broken:
                default:
                    return false;
            }
            
        }
    }
}
