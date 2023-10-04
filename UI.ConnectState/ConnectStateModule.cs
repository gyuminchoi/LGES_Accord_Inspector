using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.ConnectState.ViewModels;
using UI.ConnectState.Views;

namespace UI.ConnectState
{
    public class ConnectStateModule : IModule
    {
        private IRegionManager _regionManager;

        public ConnectStateModule(IRegionManager rm)
        {
            _regionManager = rm;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RequestNavigate("ConnectStateRegion", nameof(ConnectStateUserControl));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ConnectStateUserControl, ConnectStateViewModel>();
        }
    }
}
