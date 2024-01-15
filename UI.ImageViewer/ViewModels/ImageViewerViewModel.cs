using BarcodeLabel.Core.Events;
using Prism.Events;
using Prism.Mvvm;
using Service.Camera.Services.ConvertService;
using Service.Logger.Services;
using Service.Postprocessing.Models;
using Service.Postprocessing.Services;
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
        private IPostprocessingManager _postprocessing;
        private BitmapConverter _bmpConverter = BitmapConverter.Instance;
        private ServicesInitCompleteEvent _servicesInitCompleteEvent;
        private LogWrite _logWrite = LogWrite.Instance;
        #endregion

        #region 프로퍼티
        public int TopBoxCount { get => _topBoxCount; set => SetProperty(ref _topBoxCount, value); }
        public int TopBarcodeCount { get => _topBarcodeCount; set => SetProperty(ref _topBarcodeCount, value); }
        public BitmapImage TopOverlayImg { get => _topOverlayImg; set => SetProperty(ref _topOverlayImg, value); }
        public int BottomBoxCount { get => _bottomBoxCount; set => SetProperty(ref _bottomBoxCount, value); }
        public int BottomBarcodeCount { get => _bottomBarcodeCount; set => SetProperty(ref _bottomBarcodeCount, value); }
        public BitmapImage BottomOverlayImg { get => _bottomOverlayImg; set => SetProperty(ref _bottomOverlayImg, value); }
        #endregion

        #region 생성자
        public ImageViewerViewModel(IEventAggregator ea, IPostprocessingManager postprocessingManager)
        {
            _eventAggregator = ea;
            _postprocessing = postprocessingManager;

            _servicesInitCompleteEvent = _eventAggregator.GetEvent<ServicesInitCompleteEvent>();
            _servicesInitCompleteEvent.Subscribe(OnSubscribeEvent);
        }


        #endregion

        #region 메서드
        private void OnSubscribeEvent()
        {
            _postprocessing.ProcessorDic["Top"].PostprocessComplete += OnTopImageUpdate;
            _postprocessing.ProcessorDic["Bottom"].PostprocessComplete += OnBottomImageUpdate;
        }

        private void OnTopImageUpdate(PostprocessingResult result)
        {
            TopBarcodeCount = 0;

            TopBoxCount = result.VisionProResult.BoxDatas.Count;
            foreach (var box in result.VisionProResult.BoxDatas)
            {
                TopBarcodeCount += box.Barcodes.Count;
            }

            TopOverlayImg = _bmpConverter.BitmapToBitmapImage(result.OverlayBmp);
            result.Dispose();
            //result.OverlayBmp.Save($@"D:\Daewon\TestImage\{_count}.bmp", ImageFormat.Bmp);
            //lock (_testLock)
            //{
            //    _count++;
            //}
        }
        private void OnBottomImageUpdate(PostprocessingResult result)
        {
            BottomBarcodeCount = 0;

            BottomBoxCount = result.VisionProResult.BoxDatas.Count;
            foreach (var box in result.VisionProResult.BoxDatas)
            {
                BottomBarcodeCount += box.Barcodes.Count;
            }

            BottomOverlayImg = _bmpConverter.BitmapToBitmapImage(result.OverlayBmp);

            result.Dispose();
            //result.OverlayBmp.Save($@"D:\Daewon\TestImage\{_count}.bmp",ImageFormat.Bmp);
            //lock (_testLock)
            //{
            //    _count++;
            //}
        }


        #endregion
    }
}
