using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Setting.Models
{
    public class IOSetting : BindableBase
    {
        private string _ipAddress;
        private IOData _cam1Trigger;
        private IOData _cam2Trigger;
        private IOData _cam3Trigger;
        private IOData _cam4Trigger;
        private IOData _cam1RetryTrig;
        private IOData _cam2RetryTrig;
        private IOData _cam3RetryTrig;
        private IOData _cam4RetryTrig;

        public string IPAddress { get => _ipAddress; set => SetProperty(ref _ipAddress, value); }   

        public IOData Cam1Trigger { get => _cam1Trigger; set => SetProperty(ref _cam1Trigger, value); }
        public IOData Cam2Trigger { get => _cam2Trigger; set => SetProperty(ref _cam2Trigger, value); }
        public IOData Cam3Trigger { get => _cam3Trigger; set => SetProperty(ref _cam3Trigger, value); }
        public IOData Cam4Trigger { get => _cam4Trigger; set => SetProperty(ref _cam4Trigger, value); }

        public IOData Cam1RetryTrig { get => _cam1RetryTrig; set => SetProperty(ref _cam1RetryTrig, value); }
        public IOData Cam2RetryTrig { get => _cam2RetryTrig; set => SetProperty(ref _cam2RetryTrig, value); }
        public IOData Cam3RetryTrig { get => _cam3RetryTrig; set => SetProperty(ref _cam3RetryTrig, value); }
        public IOData Cam4RetryTrig { get => _cam4RetryTrig; set => SetProperty(ref _cam4RetryTrig, value); }



    }
}
