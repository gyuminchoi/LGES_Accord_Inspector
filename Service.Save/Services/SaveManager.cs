using Service.Camera.Services.ConvertService;
using Service.Database.Models;
using Service.Database.Services;
using Service.Logger.Services;
using Service.Postprocessing.Models;
using Service.Postprocessing.Services;
using Service.Setting.Models;
using Service.Setting.Services;
using Service.VisionPro.Models;
using Service.VisionPro.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using Encoder = System.Drawing.Imaging.Encoder;

namespace Service.Save.Services
{
    public class SaveManager : ISaveManager
    {
        private LogWrite _logWrite = LogWrite.Instance;
        private ISettingManager _settingManager;
        private IVisionProManager _vpManager;
        private ISQLiteManager _sqliteManager;
        private BitmapConverter _bmpConverter = new BitmapConverter();
        private Thread _saveThread = new Thread(() => { });
        private AutoResetEvent _saveDataSyncEvent = new AutoResetEvent(false);
        public ConcurrentQueue<VisionProResult> SaveQueue { get; set; } = new ConcurrentQueue<VisionProResult>();
        public bool IsRun { get; set; } = false;
        public void Initialize(ISettingManager sm, IVisionProManager vpm, ISQLiteManager sqliteManager)
        {
            _settingManager = sm;
            _vpManager = vpm;
            _sqliteManager = sqliteManager;
        }

        public void Start()
        {
            if (SaveQueue.Count > 0)
            {
                for (int i = 0; i < SaveQueue.Count; i++)
                {
                    SaveQueue.TryDequeue(out VisionProResult result);
                    result.Dispose();
                }
            }

            IsRun = true;

            _saveThread = new Thread(new ThreadStart(SaveProcess));
            _saveThread.Name = "Save Thread";
            _saveThread.Start();
        }

        public void Stop()
        {
            try
            {
                if (!_saveThread.IsAlive)
                    return;

                IsRun = false;

                if (_saveThread.Join(1000))
                    _saveThread.Abort();

                if (SaveQueue.Count > 0)
                {
                    for (int i = 0; i < SaveQueue.Count; i++)
                    {
                        SaveQueue.TryDequeue(out VisionProResult result);
                        result.Dispose();
                    }
                }
            }
            catch (Exception err)
            {
                _logWrite?.Error(err);
            }
        }

        public void DataSave(VisionProResult result)
        {
            string settingPath = _settingManager.AppSetting.ImageSetting.InspectionImageSavePath;
            string directory = SetDirectory(settingPath, result.InspectionTime);
            Directory.CreateDirectory(directory);
            Save(result, directory, _settingManager.AppSetting.ImageSetting);
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

        private void SaveProcess()
        {
            Stopwatch sw = new Stopwatch();
            while (IsRun)
            {

                if(!SaveQueue.TryDequeue(out VisionProResult result))
                {
                    Thread.Sleep(10);
                    continue;
                }
                sw.Restart();

                DataSave(result);

                result.Dispose();

                sw.Stop();
                _logWrite?.Info($"Data Save Complete!!, Processing time : {sw.ElapsedMilliseconds}", false, false);
            }
        }

        private string SetDirectory(string rootDirectory, DateTime dt)
        {
            string dateDirectory = DateTimeToPath(dt);
            string saveDirectory = Path.Combine(rootDirectory, dateDirectory);
            return saveDirectory;
        }

        private void Save(VisionProResult result, string directoryPath, ImageSetting setting)
        {
            string filePath = null;
            Stopwatch sw = new Stopwatch();
            try
            {
                foreach (var boxData in result.BoxDatas)
                {
                    filePath = SetFilePath(boxData, setting, directoryPath);

                    // Image Save
                    if (setting.IsCompression.Value)
                    {
                        sw.Restart();
                        ImageSaveAsJpg(filePath, boxData.CropBmp, 80L);
                        sw.Stop();
                        _logWrite?.Info($"Compression Save Processing time : {sw.ElapsedMilliseconds}", false, false);
                    }
                    else
                    {
                        //TODO : Test Code
                        sw.Restart();
                        Bitmap bmp = _bmpConverter.Get8GrayBitmap(boxData.CropBmp);
                        sw.Stop();
                        _logWrite?.Info($"24Bit -> 8Bit, Processing time : {sw.ElapsedMilliseconds}", false, false);

                        sw.Restart();
                        bmp.Save(filePath, ImageFormat.Bmp);
                        sw.Stop();
                        _logWrite?.Info($"Bitmap Save, Processing time : {sw.ElapsedMilliseconds}", false, false);
                        bmp.Dispose();
                    }

                    // 데이터 정렬
                    string parcelCode = boxData.Barcodes.First().Code;
                    string productCode = boxData.Barcodes.Last().Code;

                    // Insert DB Table 
                    sw.Restart();
                    _sqliteManager.InsertData(new RecordData(result.InspectionTime, parcelCode, productCode, filePath));
                    sw.Stop();
                    _logWrite?.Info($"DB Insert, Processing time : {sw.ElapsedMilliseconds}", false, false);
                }
            }
            catch (Exception err)
            {
                _logWrite?.Info(filePath);
                _logWrite?.Error(err);
            }
            
        }

        private string SetFilePath(Box boxData, ImageSetting setting, string directoryPath)
        {
            string parcelCode = boxData.Barcodes.First().Code;
            string productCode = boxData.Barcodes.Last().Code;

            string extension = null;
            if (setting.IsCompression.Value) extension = ".jpg";
            else extension = ".bmp";

            string fileName = $"{parcelCode}_{productCode}{extension}";
            string filePath = Path.Combine(directoryPath, fileName);

            return filePath;
        }

        private string DateTimeToPath(DateTime dt)
        {
            string dateStr = dt.ToString("yyyy-MM-dd");
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
