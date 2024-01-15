using BarcodeLabel.Core.Events;
using BarcodeLabel.Main.Views;
using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Service.Camera.Models;
using Service.Camera.Services;
using Service.ConnectionCheck.Services;
using Service.Database.Models;
using Service.Database.Services;
using Service.IO.Services;
using Service.Logger.Services;
using Service.Postprocessing.Services;
using Service.Setting.Services;
using Service.VisionPro.Services;
using Services.ImageMerge.Services;
using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using UI.ConnectState;
using UI.Controller;
using UI.ImageViewer;
using UI.LogBoard;
using UI.Measurement;
using UI.MenuBar.Modules;

namespace BarcodeLabel.Main.Services.Bootstrappers
{
    internal class Bootstrapper : PrismBootstrapper
    {
        private readonly string[] _possibleViews = new[] { "Window", "Dialog", "View", "UserControl" };
        private LogWrite _logWrite = LogWrite.Instance;
        private IOManager _ioManager = IOManager.Instance;
        

        protected override DependencyObject CreateShell() => Container.Resolve<MainWindow>();

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<ISettingManager, SettingManager>();
            containerRegistry.RegisterSingleton<ICameraManager, CameraManager>();
            containerRegistry.RegisterSingleton<IImageMergeManager, ImageMergeManager>();
            containerRegistry.RegisterSingleton<IVisionProManager, VisionProManager>();
            containerRegistry.RegisterSingleton<IPostprocessingManager, PostprocessingManager>();
            containerRegistry.RegisterSingleton<IConnectionCheckManager, ConnectionCheckManager>();
            containerRegistry.RegisterSingleton<ISQLiteManager,SQLiteManager>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);

            moduleCatalog.AddModule<MenuBarModule>();
            moduleCatalog.AddModule<ConnectStateModule>();
            moduleCatalog.AddModule<LogBoardModule>();
            moduleCatalog.AddModule<MeasurementModule>();
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
            settingManager.Load();
            _logWrite.Info("Initialize Setting Manager Complete!");

            ICameraManager camManager = Container.Resolve<ICameraManager>();
            camManager.Opens();
            _logWrite.Info("Initialize Camera Manager Complete!");

            bool isOpen = _ioManager.Open(settingManager.AppSetting.IOSetting.IPAddress);
            if (isOpen)
            {
                _ioManager.Start(true);
                _logWrite?.Info("Initialize IO Manager Complete!");
            }

            IImageMergeManager imManager = Container.Resolve<IImageMergeManager>();
            imManager.Initialize(camManager);
            _logWrite.Info("Initialize Preprocessing Manager Complete!");

            IVisionProManager vpManager = Container.Resolve<IVisionProManager>();
            vpManager.Initialize(imManager);
            _logWrite.Info("Initialize VisionPro Manager Complete!");

            IPostprocessingManager postprocessingManager = Container.Resolve<IPostprocessingManager>();
            postprocessingManager.Initialize(vpManager);
            _logWrite.Info("Initialize Postprocessing Manager Complete!");

            ISQLiteManager sqliteManager = Container.Resolve<ISQLiteManager>();
            sqliteManager.Initialize(settingManager);
            _logWrite.Info("Initialize SQLite Manager Complete!");

            IConnectionCheckManager ccManager = Container.Resolve<IConnectionCheckManager>();
            ccManager.Initialize(camManager, vpManager, sqliteManager);
            _logWrite.Info("Initialize Connection Check Manager Complete!");

            IEventAggregator eventAggregator = Container.Resolve<IEventAggregator>();
            eventAggregator.GetEvent<ServicesInitCompleteEvent>().Publish();
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
