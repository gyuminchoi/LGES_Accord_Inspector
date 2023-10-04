using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.Measurement.ViewModels;
using UI.Measurement.Views;

namespace UI.Measurement
{
    public class MeasurementModule : IModule
    {
        private IRegionManager _regionManager;

        public MeasurementModule(IRegionManager rm)
        {
            _regionManager = rm;
        }
        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RequestNavigate("MeasurementRegion", nameof(MeasurementUserControl));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MeasurementUserControl, MeasurementViewModel>();
        }
    }
}
