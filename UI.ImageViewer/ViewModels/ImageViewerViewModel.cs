using CREVIS_SWIR_Inspector.Core.Events;
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
        private IEventAggregator _eventAggregator;
        private IPostprocessingManager _ppManager;
        private ICameraManager _camManager;
        private ServicesInitCompleteEvent _servicesInitCompleteEvent;
        private LogWrite _logWrite = LogWrite.Instance;

        private WriteableBitmap _swirOriginImg;
        private WriteableBitmap _swirOverlayImg;
        private int _swirCount = 0;

        private WriteableBitmap _standardOriginImg;
        private WriteableBitmap _standardOverlayImg;
        private int _standardCount = 0;
        #endregion

        #region 프로퍼티
        // SWIR
        public WriteableBitmap SWIROriginImg { get => _swirOriginImg; set => SetProperty(ref _swirOriginImg, value); }
        public WriteableBitmap SWIROverlayImg { get => _swirOverlayImg; set => SetProperty(ref _swirOverlayImg, value); }
        public int SWIRCount { get => _swirCount; set => SetProperty(ref _swirCount, value); }

        // Standard
        public WriteableBitmap StandardOriginImg { get => _standardOriginImg; set => SetProperty(ref _standardOriginImg, value); }
        public WriteableBitmap StandardOverlayImg { get => _standardOverlayImg; set => SetProperty(ref _standardOverlayImg, value); }
        public int StandardCount { get => _standardCount; set => SetProperty(ref _standardCount, value); }

        
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
            try
            {
                
                _ppManager.ProcessorDic["SWIR"].DisplayUpdateEvent += OnSWIRUIUpdate;
                _ppManager.ProcessorDic["Standard"].DisplayUpdateEvent += OnStandardUIUpdate;

                ICamera swirCam = _camManager.CameraDic["SWIR"];
                ICamera standardCam = _camManager.CameraDic["Standard"];

                SWIROriginImg = new WriteableBitmap(swirCam.CamConfig.Height, swirCam.CamConfig.Width, 96, 96, PixelFormats.Indexed8, BitmapPalettes.Gray256);
                SWIROverlayImg = new WriteableBitmap(swirCam.CamConfig.Height, swirCam.CamConfig.Width, 96, 96, PixelFormats.Rgb24, null);

                StandardOriginImg = new WriteableBitmap(standardCam.CamConfig.Height, standardCam.CamConfig.Width, 96, 96, PixelFormats.Indexed8, BitmapPalettes.Gray256);
                StandardOverlayImg = new WriteableBitmap(standardCam.CamConfig.Height, standardCam.CamConfig.Width, 96, 96, PixelFormats.Rgb24, null);
            }
            catch (Exception err)
            {
                _logWrite?.Error(err);
            }
            
        }

        private void OnSWIRUIUpdate(DisplayData displayData)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                SWIROriginImg.Lock();
                CopyMemory(SWIROriginImg.BackBuffer, displayData.OriginPImage, (uint)_camManager.CameraDic["SWIR"].CamConfig.Buffersize);
                SWIROriginImg.AddDirtyRect(new Int32Rect(0, 0, SWIROriginImg.PixelWidth, SWIROriginImg.PixelHeight));
                SWIROriginImg.Unlock();

                SWIROverlayImg.Lock();
                CopyMemory(SWIROverlayImg.BackBuffer, displayData.OverlayPImage, (uint)_camManager.CameraDic["SWIR"].CamConfig.Buffersize * 3);
                SWIROverlayImg.AddDirtyRect(new Int32Rect(0, 0, SWIROverlayImg.PixelWidth, SWIROverlayImg.PixelHeight));
                SWIROverlayImg.Unlock();
            }));

            SWIRCount++;
        }

        private void OnStandardUIUpdate(DisplayData displayData)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                StandardOriginImg.Lock();
                CopyMemory(StandardOriginImg.BackBuffer, displayData.OriginPImage, (uint)_camManager.CameraDic["Standard"].CamConfig.Buffersize);
                StandardOriginImg.AddDirtyRect(new Int32Rect(0, 0, StandardOriginImg.PixelWidth, StandardOriginImg.PixelHeight));
                StandardOriginImg.Unlock();

                StandardOverlayImg.Lock();
                CopyMemory(StandardOverlayImg.BackBuffer, displayData.OverlayPImage, (uint)_camManager.CameraDic["Standard"].CamConfig.Buffersize * 3);
                StandardOverlayImg.AddDirtyRect(new Int32Rect(0, 0, StandardOverlayImg.PixelWidth, StandardOverlayImg.PixelHeight));
                StandardOverlayImg.Unlock();
            }));
              
            StandardCount++;
        }

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
        #endregion
    }
}
