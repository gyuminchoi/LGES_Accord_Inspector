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
    public class ImageSetting : BindableBase
    {
        private LogWrite _logWrite = LogWrite.Instance;
        private bool? _isSaveOverlay;
        private bool? _isSaveOriginal;
        private string _savePath;

        public bool? IsSaveOverlay { get => _isSaveOverlay; set => SetProperty(ref _isSaveOverlay, value); }
        public bool? IsSaveOriginal { get => _isSaveOriginal; set => SetProperty(ref _isSaveOriginal, value); }
        public string SavePath { get => _savePath; set => SetProperty(ref _savePath, value); }
        
        public ImageSetting(bool isSaveOverlay, bool isSaveOriginal, string savePath) 
        {
            IsSaveOverlay = isSaveOverlay;
            IsSaveOriginal = isSaveOriginal;
            SavePath = savePath;
        }

        public ImageSetting() { }

        public void CreateDirectory()
        {
            try
            {
                if (Path.IsPathRooted(SavePath))
                    Directory.CreateDirectory(SavePath);
            }
            catch (ArgumentException) { _logWrite.Info("Image save directory is invalid path.", true); }
            catch (Exception err) { _logWrite.Error(err); }
        }
    }
}
