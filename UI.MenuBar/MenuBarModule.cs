using Dialog.AppSetting.ViewModels;
using Dialog.AppSetting.Views;
using Dialog.DirectorySelecton.Views;
using Dialog.LiveCam.ViewModels;
using Dialog.LiveCam.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
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
            containerRegistry.RegisterForNavigation<DirectorySelectionWindow>();
            containerRegistry.RegisterDialog<AppSettingDialog, AppSettingViewModel>();
            containerRegistry.RegisterDialog<LiveCamDialog, LiveCamViewModel>();
        }
    }
}
