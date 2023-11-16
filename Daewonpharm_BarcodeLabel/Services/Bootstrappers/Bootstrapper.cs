using BarcodeLabel.Core.Events;
using BarcodeLabel.Main.Views;
using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Service.Camera.Models;
using Service.Camera.Services;
using Service.IO.Services;
using Service.Logger.Services;
using Service.Setting.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            containerRegistry.RegisterSingleton<ICameraManager, CameraManager>();
            containerRegistry.RegisterSingleton<ISettingManager, SettingManager>();
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
            _logWrite.Info("SplashScreen Start!");
            SplashScreen sc = new SplashScreen(@"/Resources/Images/checkbox_logo.png");
            sc.Show(false);

            base.InitializeModules();

            Thread.Sleep(1000);

            ISettingManager settingManager = Container.Resolve<ISettingManager>();
            settingManager.Initialize();
            settingManager.Deserialize();
            _logWrite.Info("Initialize Setting Manager Complete!");

            ICameraManager camManager = Container.Resolve<ICameraManager>();
            camManager.Opens();
            _logWrite.Info("Initialize Camera Manager Complete!");

            //bool isOpen = _ioManager.Open(settingManager.AppSetting.IOSetting.IPAddress);
            //if (isOpen)
            //{
            //    _ioManager.Start(true);
            //    //TODO : IO Test
            //    //_ioManager.WriteThread();
            //    _logWrite?.Info("Initialize IO Manager Complete!");
            //}

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
