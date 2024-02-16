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
        private bool? _isCompression;
        private bool? _isSaveImage;
        private string _inspectionImageSavePath;
        private string _liveImageSavePath;
        public bool? IsCompression { get => _isCompression; set => SetProperty(ref _isCompression, value); }
        public bool? IsSaveImage { get => _isSaveImage; set => SetProperty(ref _isSaveImage, value); }
        public string InspectionImageSavePath { get => _inspectionImageSavePath; set => SetProperty(ref _inspectionImageSavePath, value); }
        public string LiveImageSavePath { get => _liveImageSavePath; set => SetProperty(ref _liveImageSavePath, value); }

        public ImageSetting(bool isCompression, bool isSaveImage, string inspectionImageSavePath, string liveImageSavePath) 
        {
            IsCompression = isCompression;
            IsSaveImage = isSaveImage;
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
