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
        private string _liveImageSavePath;

        public string LiveImageSavePath { get => _liveImageSavePath; set => SetProperty(ref _liveImageSavePath, value); }
        public string LogSavePath { get => _logSavePath; set => SetProperty(ref _logSavePath, value); }

        public GeneralSetting(string logSavePath, string liveImageSavePath)
        {
            LogSavePath = logSavePath;
            LiveImageSavePath = liveImageSavePath;
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

            try
            {
                if (Path.IsPathRooted(LiveImageSavePath))
                    Directory.CreateDirectory(LiveImageSavePath);
            }
            catch (ArgumentException) { _logWrite.Info("Live Image is invalid path.", true); }
            catch (Exception err) { _logWrite.Error(err); }
        }
    }
}
