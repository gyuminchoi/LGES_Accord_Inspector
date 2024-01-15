using Crevis.VirtualFG40Library;
using Prism.Commands;
using Prism.Mvvm;
using Service.Camera.Models;
using Service.Camera.Services;
using Service.Camera.Services.ConvertService;
using Service.Logger.Services;
using Service.Setting.Models;
using Service.Setting.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace Dialog.LiveCam.Models
{
    public class LiveCamera : BindableBase, IDisposable
    {
        private BitmapConverter _bmpConverter = BitmapConverter.Instance;
        private LogWrite _logWrite = LogWrite.Instance;
        private ICamera _camera;
        private ImageSetting _imageSetting;
        private BitmapImage _imageViewer;
        private ulong _packetLossCount;
        private int _row;
        private int _col;
        private int _currentGain;
        private double _currentExeTime;
        private object _saveLock = new object();
        private int _frameCount = 0;

        public ICamera Camera { get => _camera; set => SetProperty(ref _camera, value); }
        public BitmapImage ImageViewer { get => _imageViewer; set => SetProperty(ref _imageViewer, value); }
        public int CurrentGain { get => _currentGain; set => SetProperty(ref _currentGain, value); }
        public double CurrentExeTime 
        {
            get => _currentExeTime; 
            set => SetProperty(ref _currentExeTime, Math.Round(value, 3)); 
        }
        public int Row { get => _row; set => SetProperty(ref _row, value); }
        public int Col { get => _col; set => SetProperty(ref _col, value); }
        public int FrameCount { get => _frameCount; set => SetProperty(ref _frameCount, value); }
        //public ulong PacketLossCount { get => _packetLossCount; set => SetProperty(ref _packetLossCount, value); }

        public DelegateCommand<object> TxbGainKeyDownCommand => new DelegateCommand<object>(OnChageGainVal);
        public DelegateCommand<object> TxbExpTimeKeyDownCommand => new DelegateCommand<object>(OnChangeExeposureTimeVal);
        public DelegateCommand BtnImageSaveCommand => new DelegateCommand(OnSaveImage);

        
        public LiveCamera(ICamera cam, ImageSetting @is)
        {
            Camera = cam;
            _imageSetting = @is;

            CurrentExeTime = Camera.CamConfig.ExposeureTime;
            CurrentGain = Camera.CamConfig.Gain;

            Camera.ReceiveRawDataEnqueueComplete += DataEnqueueComplete;

            //Thread thread = new Thread(() => 
            //{
            //    while (true) 
            //    {
            //        PacketLossCount = Camera.GetPacketLossCount();
            //        Thread.Sleep(1000);
            //    }
            //});
            //thread.Start();
        }
        public void SaveParameter()
        {
            _camera.CamConfig.Gain = CurrentGain;
            _camera.CamConfig.ExposeureTime = CurrentExeTime;
        }

        public void RollbackParameter()
        {
            _camera.SetParameter(ECameraGetSetType.VFGInt, VirtualFG40Library.MCAM_GAIN_RAW, _camera.CamConfig.Gain);
            _camera.SetParameter(ECameraGetSetType.VFGFloat, VirtualFG40Library.MCAM_EXPOSURE_TIME, _camera.CamConfig.ExposeureTime);
        }

        private void OnChangeExeposureTimeVal(object pressKey)
        {
            if ((Key)pressKey != Key.Enter)
                return;

            if(CurrentExeTime < 500)
            {
                _logWrite.Info("Please enter more than 500.0", true);
                return;
            }

            _camera.SetParameter(ECameraGetSetType.VFGFloat, VirtualFG40Library.MCAM_EXPOSURE_TIME, CurrentExeTime);
        }

        
        private void OnSaveImage()
        {
            try
            {
                BitmapImage bmpImage = null;
                lock (_saveLock)
                {
                    bmpImage = ImageViewer.Clone();
                }

                Bitmap bmp = _bmpConverter.BitmapImageTo8BitBitmap(bmpImage);

                string directory = CreateDirectory();
                string filePath = CreateFilePath(directory);

                bmp.Save(filePath, ImageFormat.Bmp);

                _logWrite.Info($"Success {Camera.CamConfig.UserID} Image Save!");
            }
            catch (Exception err)
            {
                _logWrite.Error(err);
            }
        }

        private string CreateDirectory()
        {
            string date = DateTime.Now.ToString("yy_MM_dd");
            string directory = Path.Combine(_imageSetting.LiveImageSavePath, date);

            Directory.CreateDirectory(directory);
            return directory;
        }

        private string CreateFilePath(string directory)
        {
            string filePath = Path.Combine(directory, DateTime.Now.ToString("yy_MM_dd_HH_mm_ss") + $"_{Camera.CamConfig.UserID}.bmp");

            return filePath;
        }

        private void OnChageGainVal(object pressKey)
        {
            if ((Key)pressKey != Key.Enter)
                return;

            if (CurrentGain > 1957)
            {
                _logWrite.Info("Please enter less than 1957", true);
                return;
            }

            _camera.SetParameter(ECameraGetSetType.VFGInt, VirtualFG40Library.MCAM_GAIN_RAW, CurrentGain);
        }

        private void DataEnqueueComplete(object sender, string e)
        {
            ThreadPool.QueueUserWorkItem(RawDataToBitmapImage, e);
        }

        private void RawDataToBitmapImage(object camUserID)
        {
            string userID = camUserID.ToString();

            if (!Camera.RawDatas.TryDequeue(out byte[] rawData))
                return;

            BitmapData bmpData = new BitmapData();
            bmpData.Stride = _camera.CamConfig.Stride;
            bmpData.Height = _camera.CamConfig.Height;
            bmpData.Width = _camera.CamConfig.Width;
            bmpData.PixelFormat = _camera.CamConfig.BitmapPixelFormat;

            Bitmap bmp = _bmpConverter.ByteArrayToBitmap(bmpData, rawData, _camera.CamConfig.BitmapPixelFormat);
            ImageViewer = _bmpConverter.BitmapToBitmapImage(bmp);
            FrameCount++;
            bmp.Dispose();
        }

        public void Dispose()
        {
            Camera.ReceiveRawDataEnqueueComplete -= DataEnqueueComplete;
        }
    }
}
