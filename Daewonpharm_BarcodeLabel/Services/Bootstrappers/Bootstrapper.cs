using BarcodeLabel.Main.Views;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
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

        protected override DependencyObject CreateShell() => Container.Resolve<MainWindow>();


        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
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
            SplashScreen sc = new SplashScreen(@"/Resources/Images/checkbox_logo.png");
            sc.Show(false);
            base.InitializeModules();

            Thread.Sleep(1000);
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
