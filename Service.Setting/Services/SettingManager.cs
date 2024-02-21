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
            }

            Load();
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
            return new AppSetting()
            {
                GeneralSetting = new GeneralSetting(
                    logSavePath: _logWrite.SavePath),

                //VisionProSetting = new VisionProSetting(
                //    recipes: recipes),
                VPDLSetting = new VPDLSetting()
                {
                    WorkspacePath = @"C:\Users\TSgyuminChoi\Desktop\SWIR\SWIR_Workspace.vrws",
                    Workspacename = @"SWIR_Workspace",
                    StreamName = "default",
                    ToolName = "Analyze",
                }
              };
        }
    }
}
