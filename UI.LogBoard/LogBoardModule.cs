using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.LogBoard.ViewModels;
using UI.LogBoard.Views;

namespace UI.LogBoard
{
    public class LogBoardModule : IModule
    {
        private IRegionManager _regionManager;

        public LogBoardModule(IRegionManager rm) 
        {
            _regionManager = rm;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RequestNavigate("LogBoardRegion", nameof(LogBoardUserControl));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<LogBoardUserControl, LogBoardViewModel>();
        }
    }
}
