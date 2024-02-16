using BarcodeLabel.Core.Events;
using Prism.Events;
using Prism.Mvvm;
using Service.Camera.Models;
using Service.Camera.Services.ConvertService;
using Service.Logger.Services;
using Service.Postprocessing.Models;
using Service.Postprocessing.Services;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UI.ImageViewer.ViewModels
{
    public class ImageViewerViewModel : BindableBase
    {
        #region 필드
        private int _topBoxCount = 0;
        private int _topBarcodeCount = 0;
        private WriteableBitmap _topOverlayImg;
        private int _bottomBoxCount = 0;
        private int _bottomBarcodeCount = 0;
        private WriteableBitmap _bottomOverlayImg;
        private IEventAggregator _eventAggregator;
        private IPostprocessingManager _ppManager;
        private ICameraManager _camManager;
        private ServicesInitCompleteEvent _servicesInitCompleteEvent;
        private LogWrite _logWrite = LogWrite.Instance;
        private int _topCount = 0;
        private int _botCount = 0;
        private bool _topIsPass;
        private bool _botIsPass;
        #endregion

        #region 프로퍼티
        // Top
        public int TopBoxCount { get => _topBoxCount; set => SetProperty(ref _topBoxCount, value); }
        public int TopBarcodeCount { get => _topBarcodeCount; set => SetProperty(ref _topBarcodeCount, value); }
        public WriteableBitmap TopOverlayImg { get => _topOverlayImg; set => SetProperty(ref _topOverlayImg, value); }
        public int TopCount { get => _topCount; set => SetProperty(ref _topCount, value); }
        public bool TopIsPass { get => _topIsPass; set => SetProperty(ref _topIsPass, value); }

        // Bot
        public int BottomBoxCount { get => _bottomBoxCount; set => SetProperty(ref _bottomBoxCount, value); }
        public int BottomBarcodeCount { get => _bottomBarcodeCount; set => SetProperty(ref _bottomBarcodeCount, value); }
        public WriteableBitmap BottomOverlayImg { get => _bottomOverlayImg; set => SetProperty(ref _bottomOverlayImg, value); }
        public int BotCount { get => _botCount; set => SetProperty(ref _botCount, value); }
        public bool BotIsPass { get => _botIsPass; set => SetProperty(ref _botIsPass, value); }

        private int _bufferSize = 0;
        #endregion

        #region 생성자
        public ImageViewerViewModel(IEventAggregator ea, IPostprocessingManager ppm, ICameraManager cm)
        {
            _eventAggregator = ea;
            _ppManager = ppm;
            _camManager = cm;
            _servicesInitCompleteEvent = _eventAggregator.GetEvent<ServicesInitCompleteEvent>();
            _servicesInitCompleteEvent.Subscribe(OnSubscribeEvent);
        }

        #endregion

        #region 메서드
        private void OnSubscribeEvent()
        {
            _ppManager.ProcessorDic["Top"].DisplayUpdateEvent += OnTopUIUpdate;
            _ppManager.ProcessorDic["Bottom"].DisplayUpdateEvent += OnBottomUIUpdate;

            ICamera cam = _camManager.CameraDic.Values.First();
            TopOverlayImg = new WriteableBitmap(cam.CamConfig.Height * 2, cam.CamConfig.Width, 96, 96, PixelFormats.Rgb24, null);
            BottomOverlayImg = new WriteableBitmap(cam.CamConfig.Height * 2, cam.CamConfig.Width, 96, 96, PixelFormats.Rgb24, null);
            _bufferSize = cam.CamConfig.Buffersize * 6;

            ////TODO : test
            //Bitmap bmp1 = new Bitmap(@"C:\Users\TSgyuminChoi\Desktop\대원제약 검토 자료\1101\Test2\Bot Result\1.bmp");
            //IntPtr pImage = BitmapToPtr(bmp1);

            //TopOverlayImg.Lock();
            //CopyMemory(TopOverlayImg.BackBuffer, pImage, (uint)_bufferSize);
            //TopOverlayImg.AddDirtyRect(new Int32Rect(0, 0, TopOverlayImg.PixelWidth, TopOverlayImg.PixelHeight));
            //TopOverlayImg.Unlock();

            //BottomOverlayImg.Lock();
            //CopyMemory(BottomOverlayImg.BackBuffer, pImage, (uint)_bufferSize);
            //BottomOverlayImg.AddDirtyRect(new Int32Rect(0, 0, BottomOverlayImg.PixelWidth, BottomOverlayImg.PixelHeight));
            //BottomOverlayImg.Unlock();

            TopIsPass = false;
            BotIsPass = true;
        }
        //TODO :Test
        public IntPtr BitmapToPtr(Bitmap bitmap)
        {
            BitmapData bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            bitmap.UnlockBits(bmpdata);
            return bmpdata.Scan0;
        }

        private void OnTopUIUpdate(DisplayData displayData)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                TopOverlayImg.Lock();
                CopyMemory(TopOverlayImg.BackBuffer, displayData.PImage, (uint)_bufferSize);
                TopOverlayImg.AddDirtyRect(new Int32Rect(0, 0, TopOverlayImg.PixelWidth, TopOverlayImg.PixelHeight));
                TopOverlayImg.Unlock();
            }));

            TopIsPass = displayData.IsPass;
            TopBoxCount = displayData.BoxCount;
            TopBarcodeCount = displayData.BarcodeCount;
            TopCount++;
        }

        private void OnBottomUIUpdate(DisplayData displayData)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                BottomOverlayImg.Lock();
                CopyMemory(BottomOverlayImg.BackBuffer, displayData.PImage, (uint)_bufferSize);
                BottomOverlayImg.AddDirtyRect(new Int32Rect(0, 0, BottomOverlayImg.PixelWidth, BottomOverlayImg.PixelHeight));
                BottomOverlayImg.Unlock();
            }));
            BotIsPass = displayData.IsPass;
            BottomBoxCount = displayData.BoxCount;
            BottomBarcodeCount = displayData.BarcodeCount;
            BotCount++;
        }

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
        #endregion
    }
}
