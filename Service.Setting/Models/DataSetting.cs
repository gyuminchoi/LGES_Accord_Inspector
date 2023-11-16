using Prism.Mvvm;
using Service.Logger.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Setting.Models
{
    public class DataSetting : BindableBase
    {
        private LogWrite _logWrite = LogWrite.Instance;
        private string _savePath;
        private string _sendPath;

        public string SavePath { get => _savePath; set => SetProperty(ref _savePath, value); }

        public string SendPath { get => _sendPath; set => SetProperty(ref _sendPath, value); }

        public DataSetting(string savePath, string sendPath)
        {
            _savePath = savePath;
            _sendPath = sendPath;
        }
        public DataSetting() { }

        public void CreateDirectory()
        {
            try
            {
                if (Path.IsPathRooted(SavePath))
                    Directory.CreateDirectory(SavePath);
            }
            catch (ArgumentException) { _logWrite.Info("Data save directory is invalid path.", true); }
            catch (Exception err) { _logWrite.Error(err); }
        }
    }
}
