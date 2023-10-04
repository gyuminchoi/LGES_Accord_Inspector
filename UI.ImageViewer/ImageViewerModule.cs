using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.ImageViewer.ViewModels;
using UI.ImageViewer.Views;

namespace UI.ImageViewer
{
    public class ImageViewerModule : IModule
    {
        private IRegionManager _regionManager;

        public ImageViewerModule(IRegionManager rm)
        {
            _regionManager = rm;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RequestNavigate("ImageViewerRegion", nameof(ImageViewerUserControl));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ImageViewerUserControl, ImageViewerViewModel>();
        }
    }
}
