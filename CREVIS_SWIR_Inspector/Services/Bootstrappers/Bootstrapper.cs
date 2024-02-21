using CREVIS_SWIR_Inspector.Core.Events;
using CREVIS_SWIR_Inspector.Main.Views;
using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Service.Camera.Models;
using Service.Camera.Services;
using Service.DeepLearning.Services;
using Service.Logger.Services;
using Service.MainInspection.Services;
using Service.Postprocessing.Services;
using Service.Setting.Services;
using Service.VisionPro.Services;
using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using UI.Controller;
using UI.ImageViewer;

namespace CREVIS_SWIR_Inspector.Main.Services.Bootstrappers
{
    internal class Bootstrapper : PrismBootstrapper
    {
        private readonly string[] _possibleViews = new[] { "Window", "Dialog", "View", "UserControl" };
        private LogWrite _logWrite = LogWrite.Instance;
        

        protected override DependencyObject CreateShell() => Container.Resolve<MainWindow>();

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<ICameraManager, CameraManager>();
            containerRegistry.RegisterSingleton<IPostprocessingManager, PostprocessingManager>();
            containerRegistry.RegisterSingleton<IMainInpectionManager, MainInspectionManager>();
            containerRegistry.RegisterSingleton<ISettingManager, SettingManager>();
            containerRegistry.RegisterSingleton<IVisionProManager, VisionProManager>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);

            moduleCatalog.AddModule<ImageViewerModule>();
            moduleCatalog.AddModule<ControllerModule>();
        }

        protected override void InitializeModules()
        {
            _logWrite.Info("Splash screen start!");
            SplashScreen sc = new SplashScreen(@"/Resources/Images/checkbox_logo.png");
            sc.Show(false);

            base.InitializeModules();

            Thread.Sleep(500);
            ISettingManager settingManager = Container.Resolve<ISettingManager>();
            settingManager.Initialize();
            _logWrite.Info("Initialize Setting Manager Complete!");

            ICameraManager camManager = Container.Resolve<ICameraManager>();
            camManager.Opens();
            _logWrite.Info("Initialize Camera Manager Complete!");

            //IVPDLManager vpdlManager = Container.Resolve<IVPDLManager>();
            //vpdlManager.Initialize(settingManager);
            //_logWrite.Info("Initialize VPDL Manager Complete!");

            IVisionProManager vpManager = Container.Resolve<IVisionProManager>();
            vpManager.Initialize();
            _logWrite.Info("Initialize VisionPro Manager Complete!");

            IPostprocessingManager ppManager = Container.Resolve<IPostprocessingManager>();
            ppManager.Initialize();
            _logWrite.Info("Initialize Postprocessing Manager Complete!");

            IEventAggregator eventAggregator = Container.Resolve<IEventAggregator>();
            eventAggregator.GetEvent<ServicesInitCompleteEvent>().Publish();

            IMainInpectionManager miManager = Container.Resolve<IMainInpectionManager>();
            miManager.Initialize(camManager, vpManager, ppManager);
            _logWrite.Info("Initialize main inspection manager!");

            sc.Close(TimeSpan.Zero);
        }

        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();

            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver((viewType) =>
            {
                var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
                var viewName = viewType.FullName?.Replace(".Views.", ".ViewModels.");
                var viewModelName = string.Empty;

                foreach (var endViewName in _possibleViews)
                {
                    if (viewName.EndsWith(endViewName))
                    {
                        viewModelName = viewName.Substring(0, viewName.LastIndexOf(endViewName, StringComparison.Ordinal));
                        break;
                    }
                }

                var viewModelFullName = $"{viewModelName}ViewModel, {viewAssemblyName}";
                return Type.GetType(viewModelFullName);
            });
        }
    }
}
