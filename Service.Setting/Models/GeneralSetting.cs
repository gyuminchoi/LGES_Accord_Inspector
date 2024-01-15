using Prism.Mvvm;
using Service.Logger.Services;
using System.IO;
using System;

namespace Service.Setting.Models
{
    public class GeneralSetting : BindableBase
    {
        private LogWrite _logWrite = LogWrite.Instance;
        private string _logSavePath;
        public string LogSavePath { get => _logSavePath; set => SetProperty(ref _logSavePath, value); }

        public GeneralSetting(string logSavePath)
        {
            LogSavePath = logSavePath;
        }

        public GeneralSetting() { }

        public void CreateDirectory()
        {
            try
            {
                if (Path.IsPathRooted(LogSavePath))
                    Directory.CreateDirectory(LogSavePath);
            }
            catch (ArgumentException) { _logWrite.Info("Log save directory is invalid path.", true); }
            catch (Exception err) { _logWrite.Error(err); }
        }
    }
}
