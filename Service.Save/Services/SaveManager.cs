using Service.Database.Models;
using Service.Database.Services;
using Service.Logger.Services;
using Service.Postprocessing.Models;
using Service.Postprocessing.Services;
using Service.Setting.Models;
using Service.Setting.Services;
using Service.VisionPro.Models;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Encoder = System.Drawing.Imaging.Encoder;

namespace Service.Save.Services
{
    public class SaveManager : ISaveManager
    {
        private LogWrite _logWrite = LogWrite.Instance;
        private ISettingManager _settingManager;
        private IPostprocessingManager _ppManager;
        private ISQLiteManager _sqliteManager;
        private int _testCount1 = 0;
        private int _testCount2 = 0;
        private object _testLock1 = new object();
        private object _testLock2 = new object();
        public SaveManager() { }

        public void Initialize(ISettingManager settingManager, IPostprocessingManager ppManager, ISQLiteManager sqliteManager)
        {
            _settingManager = settingManager;
            _ppManager = ppManager;
            _sqliteManager = sqliteManager;
        }

        public void Start()
        {
            if (_ppManager.ProcessorDic.Count > 0)
            {
                foreach (var processor in _ppManager.ProcessorDic.Values)
                {
                    //TODO :Test
                    processor.PostprocessComplete += OnSaveInspectData2;
                }
            }
        }

        public void Stop()
        {
            if (_ppManager.ProcessorDic.Count > 0)
            {
                foreach (var processor in _ppManager.ProcessorDic.Values)
                {
                    //TODO: Test
                    processor.PostprocessComplete -= OnSaveInspectData2;
                }
            }
        }

        public void Dispose()
        {
            try
            {
                Stop();
            }
            catch (Exception err)
            {
                _logWrite.Error(err);
            }
        }

        private void OnSaveInspectData(PostprocessingResult result)
        {
            string settingPath = _settingManager.AppSetting.ImageSetting.InspectionImageSavePath;
            string directory = SetDirectory(settingPath, result.VisionProResult.InspectionTime);

            Save(result, directory, _settingManager.AppSetting.ImageSetting);
        }
        //TODO : Test
        private void OnSaveInspectData2(PostprocessingResult result)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            string settingPath = _settingManager.AppSetting.ImageSetting.InspectionImageSavePath;
            string directory = SetDirectory(settingPath, result.VisionProResult.InspectionTime);
            Directory.CreateDirectory(directory);
            Save2(result, directory, _settingManager.AppSetting.ImageSetting);
            sw.Stop();
            _logWrite.Info("Save : " + sw.ElapsedMilliseconds.ToString());
        }

        private string SetDirectory(string rootDirectory, DateTime dt)
        {
            string dateDirectory = DateTimeToPath(dt);
            string saveDirectory = Path.Combine(rootDirectory, dateDirectory);
            return saveDirectory;
        }

        private void Save(PostprocessingResult ppResult, string path, ImageSetting setting)
        {
            foreach (var boxData in ppResult.VisionProResult.BoxDatas)
            {
                string filePath = SetFilePath(boxData, setting, path);

                // Image Save
                if (setting.IsCompression.Value) 
                    ImageSaveAsJpg(filePath, boxData.CropBmp, 80L);
                else 
                    boxData.CropBmp.Save(filePath, ImageFormat.Bmp);

                // Insert DB Table 
                string parcelCode = boxData.Barcodes.First().Code;
                string productCode = boxData.Barcodes.Last().Code;

                _sqliteManager.InsertData(new RecordData(ppResult.VisionProResult.InspectionTime, parcelCode, productCode, filePath));
            }

            ppResult.Dispose();
        }
        //TODO : Test
        private void Save2(PostprocessingResult ppResult, string path, ImageSetting setting)
        {
            foreach (var boxData in ppResult.VisionProResult.BoxDatas)
            {
                string filePath = SetFilePath2(boxData, setting, path);

                // Image Save
                if (setting.IsCompression.Value)
                    ImageSaveAsJpg(filePath, boxData.CropBmp, 80L);
                else
                    boxData.CropBmp.Save(filePath, ImageFormat.Bmp);

                // Insert DB Table
                string parcelCode;
                string productCode;
                lock (_testLock1)
                {
                    parcelCode = _testCount1.ToString();
                    productCode = _testCount2.ToString();
                }

                _sqliteManager.InsertData(new RecordData(ppResult.VisionProResult.InspectionTime, parcelCode, productCode, filePath));
            }

            ppResult.Dispose();
        }
        //TODO :test
        private string SetFilePath2(Box boxData, ImageSetting setting, string directoryPath)
        {
            string parcelCode;
            string productCode;
            lock (_testLock1)
            {
                parcelCode = _testCount1.ToString();
                productCode = _testCount2.ToString();
            }

            string extension = null;
            if (setting.IsCompression.Value) extension = "jpg";
            else extension = "bmp";

            string fileName = $"{parcelCode}_{productCode}.{extension}";
            string filePath = Path.Combine(directoryPath, fileName);
            lock (_testLock1)
            {
                _testCount1++;
                _testCount2++;
            }
            return filePath;
        }

        private string SetFilePath(Box boxData, ImageSetting setting, string directoryPath)
        {
            string parcelCode = boxData.Barcodes.First().Code;
            string productCode = boxData.Barcodes.Last().Code;

            string extension = null;
            if (setting.IsCompression.Value) extension = ".jpg";
            else extension = ".bmp";

            string fileName = $"{parcelCode}_{productCode}.{extension}";
            string filePath = Path.Combine(directoryPath, fileName);

            return filePath;
        }

        private string DateTimeToPath(DateTime dt)
        {
            string dateStr = dt.ToString("yyyy_MM_dd");
            string timeStr = dt.ToString("HH_mm_ss_fff");

            string path = Path.Combine(dateStr, timeStr);
            return path;
        }

        private void ImageSaveAsJpg(string path, Bitmap image, long quality)
        {
            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            Encoder myEncoder = Encoder.Quality;

            using (var myEncoderParameters = new EncoderParameters(1))
            {
                using (var myEncoderParameter = new EncoderParameter(myEncoder, quality))
                {
                    myEncoderParameters.Param[0] = myEncoderParameter;
                    image.Save(path, jpgEncoder, myEncoderParameters);
                }
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            return codecs.Single(codec => codec.FormatID == format.Guid);
        }
    }
}
