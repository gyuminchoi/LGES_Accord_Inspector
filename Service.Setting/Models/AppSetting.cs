using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Setting.Models
{
    public class AppSetting : BindableBase
    {
        private GeneralSetting _generalSetting;
        private VisionProSetting _visionProSetting;
        private VPDLSetting _vpdlSetting;
        public GeneralSetting GeneralSetting { get => _generalSetting; set => SetProperty(ref _generalSetting, value); }
        public VisionProSetting VisionProSetting { get => _visionProSetting; set => SetProperty(ref _visionProSetting, value); }
        public VPDLSetting VPDLSetting { get => _vpdlSetting; set => SetProperty(ref _vpdlSetting,value); }
    }
}
