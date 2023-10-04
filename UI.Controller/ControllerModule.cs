using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.Controller.ViewModels;
using UI.Controller.Views;

namespace UI.Controller
{
    public class ControllerModule : IModule
    {
        private IRegionManager _regionManager;

        public ControllerModule(IRegionManager rm)
        {
            _regionManager = rm;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RequestNavigate("ControllerRegion", nameof(ControllerUserControl));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ControllerUserControl, ControllerViewModel>();
        }
    }
}
