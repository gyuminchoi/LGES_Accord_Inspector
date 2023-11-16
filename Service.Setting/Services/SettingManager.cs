using Newtonsoft.Json;
using Prism.Mvvm;
using Service.Logger.Services;
using Service.Setting.Models;
using System;
using System.IO;
using System.Reflection;

namespace Service.Setting.Services
{
    public class SettingManager : BindableBase, ISettingManager
    {
        private readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Assembly.GetEntryAssembly().GetName().Name, "Setting.json");
        private LogWrite _logWrite = LogWrite.Instance;
        private AppSetting _appSetting;


        public AppSetting AppSetting { get => _appSetting; set => SetProperty(ref _appSetting, value); }

        public void Initialize()
        {
            if (!File.Exists(_path))
            {
                AppSetting = InitializeAppSetting();

                Serialize();
                return;
            }
        }

        public void Deserialize()
        {
            string json = File.ReadAllText(_path);
            AppSetting = JsonConvert.DeserializeObject<AppSetting>(json);
        }

        public void Serialize()
        {
            string json = JsonConvert.SerializeObject(AppSetting);
            File.WriteAllText(_path, json);
        }

        private AppSetting InitializeAppSetting()
        {
            return new AppSetting()
            {
                ImageSetting = new ImageSetting(
                    isSaveOverlay: true,
                    isSaveOriginal: true,
                    savePath: @"D:\Daewon"),

                GeneralSetting = new GeneralSetting(
                    logSavePath: _logWrite.SavePath,
                    liveImageSavePath: @"D:\Daewon\LiveImage"),

                DataSetting = new DataSetting(
                    savePath: @"D:\Daewon",
                    sendPath: @"D:\Daewon"),

                IOSetting = new IOSetting(
                    ipAddr: "192.168.100.99")
            };
        }
    }
}
