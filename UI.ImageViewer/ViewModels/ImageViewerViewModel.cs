using BarcodeLabel.Core.Events;
using Prism.Events;
using Prism.Mvvm;
using Service.Camera.Services.ConvertService;
using Service.Logger.Services;
using Service.Postprocessing.Models;
using Service.Postprocessing.Services;
using System;
using System.Windows.Media.Imaging;

namespace UI.ImageViewer.ViewModels
{
    public class ImageViewerViewModel : BindableBase
    {
        #region 필드
        private int _topBoxCount = 0;
        private int _topBarcodeCount = 0;
        private BitmapImage _topOverlayImg;
        private int _bottomBoxCount = 0;
        private int _bottomBarcodeCount = 0;
        private BitmapImage _bottomOverlayImg;
        private IEventAggregator _eventAggregator;
        private IPostprocessingManager _ppManager;
        private ServicesInitCompleteEvent _servicesInitCompleteEvent;
        private LogWrite _logWrite = LogWrite.Instance;
        private int _topCount = 0;
        private int _botCount = 0;
        #endregion

        #region 프로퍼티
        public int TopBoxCount { get => _topBoxCount; set => SetProperty(ref _topBoxCount, value); }
        public int TopBarcodeCount { get => _topBarcodeCount; set => SetProperty(ref _topBarcodeCount, value); }
        public BitmapImage TopOverlayImg { get => _topOverlayImg; set => SetProperty(ref _topOverlayImg, value); }
        public int BottomBoxCount { get => _bottomBoxCount; set => SetProperty(ref _bottomBoxCount, value); }
        public int BottomBarcodeCount { get => _bottomBarcodeCount; set => SetProperty(ref _bottomBarcodeCount, value); }
        public BitmapImage BottomOverlayImg { get => _bottomOverlayImg; set => SetProperty(ref _bottomOverlayImg, value); }
        public int TopCount { get => _topCount; set => SetProperty(ref _topCount, value); }
        public int BotCount { get => _botCount; set => SetProperty(ref _botCount, value); }
        #endregion

        #region 생성자
        public ImageViewerViewModel(IEventAggregator ea, IPostprocessingManager ppm)
        {
            _eventAggregator = ea;
            _ppManager = ppm;

            _servicesInitCompleteEvent = _eventAggregator.GetEvent<ServicesInitCompleteEvent>();
            _servicesInitCompleteEvent.Subscribe(OnSubscribeEvent);
        }

        #endregion

        #region 메서드
        private void OnSubscribeEvent()
        {
            _ppManager.ProcessorDic["Top"].DisplayUpdateEvent += OnTopUIUpdate;
            _ppManager.ProcessorDic["Bottom"].DisplayUpdateEvent += OnBottomUIUpdate;
        }

        private void OnTopUIUpdate(DisplayData displayData)
        {
            TopOverlayImg = displayData.BmpImage;
            TopBoxCount = displayData.BoxCount;
            TopBarcodeCount = displayData.BarcodeCount;
            TopCount++;
        }
        private void OnBottomUIUpdate(DisplayData displayData)
        {
            BottomOverlayImg = displayData.BmpImage;
            BottomBoxCount = displayData.BoxCount;
            BottomBarcodeCount = displayData.BarcodeCount;
            BotCount++;
        }
        #endregion
    }
}
