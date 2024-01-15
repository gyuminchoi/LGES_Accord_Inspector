using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.ConnectionCheck.Models
{
    public class ServiceState : BindableBase
    {
        private bool _cameraState;
        private bool _ioState;
        private bool _dbState;
        private bool _sensorState;

        public bool CameraState { get => _cameraState; set => SetProperty(ref _cameraState, value); }
        public bool IOState { get => _ioState; set => SetProperty(ref _ioState, value); }
        public bool DatabaseState { get => _dbState; set => SetProperty(ref _dbState, value); }
        public bool SensorState { get => _sensorState; set => SetProperty(ref _sensorState, value); }

        public ServiceState() { }
    }
}