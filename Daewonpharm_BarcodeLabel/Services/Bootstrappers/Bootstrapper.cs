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
using Service.Database.Services;
using Service.Delete.Services;
using Service.IO.Services;
using Service.Logger.Services;
using Service.MainInspection.Services;
using Service.Postprocessing.Services;
using Service.Save.Services;
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
            containerRegistry.RegisterSingleton<ISaveManager, SaveManager>();
            containerRegistry.RegisterSingleton<IMainInpectionManager, MainInspectionManager>();
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

            ISQLiteManager sqliteManager = Container.Resolve<ISQLiteManager>();
            sqliteManager.Initialize(settingManager);
            _logWrite.Info("Initialize SQLite Manager Complete!");
            //Thread testThread = new Thread(() =>
            //{
            //    long i = 0;
            //    long j = 0;
            //    while (true)
            //    {
            //        RecordData data = new RecordData()
            //        {
            //            DateTime = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss"),
            //            ParcelBarcode = i.ToString(),
            //            ProductBarcode = j.ToString(),
            //            ImagePath = @"C:\Users\TSgyuminChoi\Desktop\대원제약 검토 자료\1101\Test2\Bot Result\1.bmp"
            //        };
            //        sqliteManager.InsertData(data);
            //        i++;
            //        j++;
            //        Thread.Sleep(200);
            //    }
            //});
            //testThread.Start();

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
            vpManager.Initialize(imManager, sqliteManager);
            _logWrite.Info("Initialize VisionPro Manager Complete!");

            IPostprocessingManager ppManager = Container.Resolve<IPostprocessingManager>();
            ppManager.Initialize(vpManager);
            _logWrite.Info("Initialize Postprocessing Manager Complete!");

            IConnectionCheckManager ccManager = Container.Resolve<IConnectionCheckManager>();
            ccManager.Initialize(camManager, vpManager, sqliteManager);
            _logWrite.Info("Initialize Connection Check Manager Complete!");

            IEventAggregator eventAggregator = Container.Resolve<IEventAggregator>();
            eventAggregator.GetEvent<ServicesInitCompleteEvent>().Publish();

            ISaveManager saveManager = Container.Resolve<ISaveManager>();
            saveManager.Initialize(settingManager, vpManager, sqliteManager);
            _logWrite.Info("Initialize Save Manager Complete!");

            IMainInpectionManager miManager = Container.Resolve<IMainInpectionManager>();
            miManager.Initialize(camManager, imManager, vpManager, ppManager, saveManager);
            _logWrite.Info("Initialize main inspection manager!");

            // 오래된 데이터 삭제
            DirectoryDeleteService dirDeleteService = DirectoryDeleteService.Instance;
            dirDeleteService.Initialize(settingManager);
            dirDeleteService.DeleteOldFolder();

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
