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
        private string _inspectionImageSavePath;
        private string _liveImageSavePath;

        public bool? IsSaveOverlay { get => _isSaveOverlay; set => SetProperty(ref _isSaveOverlay, value); }
        public bool? IsSaveOriginal { get => _isSaveOriginal; set => SetProperty(ref _isSaveOriginal, value); }
        public string InspectionImageSavePath { get => _inspectionImageSavePath; set => SetProperty(ref _inspectionImageSavePath, value); }
        public string LiveImageSavePath { get => _liveImageSavePath; set => SetProperty(ref _liveImageSavePath, value); }

        public ImageSetting(bool isSaveOverlay, bool isSaveOriginal, string inspectionImageSavePath, string liveImageSavePath) 
        {
            IsSaveOverlay = isSaveOverlay;
            IsSaveOriginal = isSaveOriginal;
            InspectionImageSavePath = inspectionImageSavePath;
            LiveImageSavePath = liveImageSavePath;
        }

        public ImageSetting() { }

        public void CreateDirectory()
        {
            try
            {
                if (Path.IsPathRooted(InspectionImageSavePath))
                    Directory.CreateDirectory(InspectionImageSavePath);
            }
            catch (ArgumentException) { _logWrite.Info("Image save directory is invalid path.", true); }
            catch (Exception err) { _logWrite.Error(err); }
        }
    }
}
