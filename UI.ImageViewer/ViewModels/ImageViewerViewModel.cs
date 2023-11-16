using BarcodeLabel.Core.Events;
using Prism.Events;
using Prism.Mvvm;
using Service.Camera.Models;
using Service.Camera.Services.ConvertService;
using Service.Logger.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UI.ImageViewer.ViewModels
{
    public class ImageViewerViewModel : BindableBase
    {
        #region 필드
        private int _topQuantity = 0;
        private BitmapImage _topOriginImg;
        private BitmapImage _topOverlayImg;
        private int _topFrameCount;
        private int _bottomQuantity = 0;
        private BitmapImage _bottomOriginImg;
        private BitmapImage _bottomOverlayImg;
        private int _bottomFrameCount;
        private ICameraManager _camManager;
        private IEventAggregator _eventAggregator;
        private InspectionStatusChangeEvent _inspectionStatusChangeEvent;
        private LogWrite _logWrite = LogWrite.Instance;
        #endregion

        #region 프로퍼티
        public int TopQuantity { get => _topQuantity; set => SetProperty(ref _topQuantity, value); }
        public BitmapImage TopOriginImg { get => _topOriginImg; set => SetProperty(ref _topOriginImg, value); }
        public BitmapImage TopOverlayImg { get => _topOverlayImg; set => SetProperty(ref _topOverlayImg, value); }
        public int TopFrameCount { get => _topFrameCount; set => SetProperty(ref _topFrameCount, value); }
        public int BottomQuantity { get => _bottomQuantity; set => SetProperty(ref _bottomQuantity, value); }
        public BitmapImage BottomOriginImg { get => _bottomOriginImg; set => SetProperty(ref _bottomOriginImg, value); }
        public BitmapImage BottomOverlayImg { get => _bottomOverlayImg; set => SetProperty(ref _bottomOverlayImg, value); }
        public int BottomFrameCount { get => _bottomFrameCount; set => SetProperty(ref _bottomFrameCount, value); }
        #endregion

        #region 생성자
        public ImageViewerViewModel(ICameraManager cm, IEventAggregator ea)
        {
            _camManager = cm;
            _eventAggregator = ea;
            _inspectionStatusChangeEvent = _eventAggregator.GetEvent<InspectionStatusChangeEvent>();
            _inspectionStatusChangeEvent.Subscribe(OnImageUpdate);
        }
        #endregion

        #region 메서드
        private void OnImageUpdate(bool obj)
        {
            //if (obj)
            //{
            //    Thread imageUpdateThread = new Thread(() =>
            //    {
            //        while (true)
            //        {
            //            _camManager.CameraDic["Cam1"].ReceiveImageDatas.TryDequeue(out Bitmap data1);
            //            if (data1 != null)
            //            {
            //                TopOriginImg = BitmapConverter.Instance.BitmapToBitmapImage(data1);
            //                TopFrameCount++;
            //                _logWrite.Info($"Top Image Update Complete!{Environment.NewLine}Top Frame Count - {TopFrameCount}");
            //            }

            //            _camManager.CameraDic["Cam2"].ReceiveImageDatas.TryDequeue(out Bitmap data2);
            //            if (data2 != null)
            //            {
            //                BottomOriginImg = BitmapConverter.Instance.BitmapToBitmapImage(data2);
            //                BottomFrameCount++;
            //                _logWrite.Info($"Bottom Image Update Complete!{Environment.NewLine}Bottom Frame Count - {BottomFrameCount}");
            //            }
            //        }
            //    });
            //    imageUpdateThread.Start();
            //}
            
        }
        #endregion
    }
}
