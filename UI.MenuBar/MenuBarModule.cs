using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.MenuBar.ViewModels;
using UI.MenuBar.Views;

namespace UI.MenuBar.Modules
{
    public class MenuBarModule : IModule
    {
        private IRegionManager _regionManager;
        public MenuBarModule(IRegionManager rm) 
        {
            _regionManager = rm;
        }
        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RequestNavigate("MenuBarRegion", nameof(MenuBarUsercontrol));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MenuBarUsercontrol, MenuBarViewModel>();
        }
    }
}
