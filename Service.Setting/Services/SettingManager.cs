using Newtonsoft.Json;
using Prism.Mvvm;
using Service.Logger.Services;
using Service.Setting.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;

namespace Service.Setting.Services
{
    public class SettingManager : BindableBase, ISettingManager
    {
        private readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Assembly.GetEntryAssembly().GetName().Name, "Setting.json");
        private LogWrite _logWrite = LogWrite.Instance;
        private AppSetting _appSetting;

        public AppSetting AppSetting { get => _appSetting; set => SetProperty(ref _appSetting, value); }

        public SettingManager()
        {
        }

        public void Initialize()
        {
            if (!File.Exists(_path))
            {
                AppSetting = InitializeAppSetting();

                Save();
                return;
            }
        }

        public void Load()
        {
            string json = File.ReadAllText(_path);
            AppSetting = JsonConvert.DeserializeObject<AppSetting>(json);
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(AppSetting);
            File.WriteAllText(_path, json);
        }

        private AppSetting InitializeAppSetting()
        {
            string barcodeColor = "Green";
            string boxColor = "Blue";
            Dictionary<string, VisionProRecipe> recipes = new Dictionary<string, VisionProRecipe>
            {
                { "3x8", new VisionProRecipe(@"D:\Daewon", @"D:\Daewon", @"D:\Daewon", 12, 4, 2, 1, 10, 400, 200, barcodeColor, boxColor) },
                { "4x10", new VisionProRecipe(@"D:\Daewon", @"D:\Daewon", @"D:\Daewon", 20, 10, 2, 1, 10, 400, 200, barcodeColor, boxColor) },
            };

            return new AppSetting()
            {
                ImageSetting = new ImageSetting(
                    isCompression: true,
                    isSaveImage: true,
                    inspectionImageSavePath: @"D:\Daewon",
                    liveImageSavePath: @"D:\Daewon"),

                GeneralSetting = new GeneralSetting(
                    logSavePath: _logWrite.SavePath),

                DataSetting = new DataSetting(
                    savePath: @"D:\Daewon",
                    sendPath: @"D:\Daewon"),

                IOSetting = new IOSetting() 
                {
                    IPAddress = "192.168.100.100",
                    Cam1Trigger = new IOData() { Slot = 0, Index = 0 },
                    Cam2Trigger = new IOData() { Slot = 0, Index = 1 },
                    Cam3Trigger = new IOData() { Slot = 0, Index = 2 },
                    Cam4Trigger = new IOData() { Slot = 0, Index = 3 },
                    Cam1RetryTrig = new IOData() { Slot = 0, Index = 4 },
                    Cam2RetryTrig = new IOData() { Slot = 0, Index = 5 },
                    Cam3RetryTrig = new IOData() { Slot = 0, Index = 6 },
                    Cam4RetryTrig = new IOData() { Slot = 0, Index = 7 },
                },

                VisionProSetting = new VisionProSetting( 
                    recipes: recipes),

                DataBaseSetting = new DataBaseSetting(
                    path: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Assembly.GetEntryAssembly().GetName().Name,"Daewon_History.db"),
                    tableName:"InspectionResult",
                    csvPath: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Assembly.GetEntryAssembly().GetName().Name,@"\CSV\"))
            };
        }
    }
}
