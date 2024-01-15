using Service.Camera.Services.ConvertService;
using Service.Logger.Services;
using Service.Save.Models;
using Service.Setting.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Encoder = System.Drawing.Imaging.Encoder;

namespace Service.Save.Services
{
    public class SaveManager : ISaveManager
    {
        private Thread _imageSaveThread = new Thread(() => { });
        private LogWrite _logWrite = LogWrite.Instance;
        private ISettingManager _settingManager;
        private BitmapConverter _bmpConverter = new BitmapConverter();
        public bool IsRun { get; set; } = false;
        /// <summary>
        /// 저장할 이미지 경로를 받아오는 큐 입니다.
        /// </summary>
        public ConcurrentQueue<ImageData> ImageSaveQueue { get; set; } = new ConcurrentQueue<ImageData>();

        public void Initialize(ISettingManager settingManager)
        {
            _settingManager = settingManager;
        }

        public void Start()
        {
            try
            {
                IsRun = true;

                _imageSaveThread = new Thread(new ThreadStart(ImageSaveProcess));
                _imageSaveThread.Name = "Image Save Thread";
                _imageSaveThread.Start();
            }
            catch (Exception err)
            {
                _logWrite?.Error(err);
                Stop();
                IsRun = false;
            }
        }

        private void ImageSaveProcess()
        {
            try
            {
                ImageData imageData = null;

                while (IsRun)
                {
                    bool flag = ImageSaveQueue.TryDequeue(out imageData);
                    if (!flag)
                    {
                        Thread.Sleep(10);
                        continue;
                    }
                    else
                    {
                        string settingPath = _settingManager.AppSetting.ImageSetting.InspectionImageSavePath;
                        string path = CreatePath(imageData, settingPath);

                        // 셋업 및 저장
                        ImageSave(imageData, path);

                        imageData.Dispose();
                    }
                }
            }
            catch (Exception err)
            {
                _logWrite?.Error(err);
            }
        }

        private string CreatePath(ImageData imageData, string rootPath)
        { 
            string dateTimePath = DateTimeToPath(imageData.InspectionTime);
            string fileName = CreateFileName(imageData);
            string path = Path.Combine(rootPath, dateTimePath, fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            return path;
        }

        public void Stop()
        {
            try
            {
                if (!_imageSaveThread.IsAlive)
                    return;

                IsRun = false;

                if (_imageSaveThread.Join(1000))
                    _imageSaveThread.Abort();
            }
            catch (Exception err)
            {
                _logWrite?.Error(err);
            }
        }

        public void Dispose()
        {
            try
            {
                Stop();

                ImageSaveQueue = null;
            }
            catch (Exception err)
            {
                _logWrite.Error(err);
            }
        }

        private void ImageSave(ImageData imageData, string path)
        {
            Bitmap bmp = imageData.Bmp;

            ImageSaveAsJpg(path, bmp, 60L);
        }

        private string CreateFileName(ImageData imageData)
        {
            string locate = imageData.Locate.ToString();                                // ex) Top
            string dateTime = imageData.InspectionTime.ToString("yyyy_MM_dd_HH_mm_ss_fff"); // ex) 2024_01-09_16_07_23

            string fileName = $"{locate}_{dateTime}__{imageData.Index}.jpg";            // ex) Top_2024_01-09_16_07_23__1.bmp

            return fileName;
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
