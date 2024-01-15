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
        private ImageSetting _imageSetting;
        private GeneralSetting _generalSetting;
        private DataSetting _dataSetting;
        private IOSetting _ioSetting;
        private VisionProSetting _visionProSetting;
        private DataBaseSetting _dataBaseSetting;

        public ImageSetting ImageSetting { get => _imageSetting; set => SetProperty(ref _imageSetting, value); }
        public GeneralSetting GeneralSetting { get => _generalSetting; set => SetProperty(ref _generalSetting, value); }
        public DataSetting DataSetting { get => _dataSetting; set => SetProperty(ref _dataSetting, value); }
        public IOSetting IOSetting { get => _ioSetting; set => SetProperty(ref _ioSetting, value); }
        public VisionProSetting VisionProSetting { get => _visionProSetting; set => SetProperty(ref _visionProSetting, value); }
        public DataBaseSetting DataBaseSetting { get => _dataBaseSetting; set => SetProperty(ref _dataBaseSetting, value); }

    }
}
