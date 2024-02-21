using Service.Camera.Services.ConvertService;
using Service.DeepLearning.Models;
using Service.Drawing.Services;
using Service.Logger.Services;
using Service.Postprocessing.Models;
using Service.VisionPro.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace Service.Postprocessing.Services
{
    public class Postprocessor : IDisposable
    {
        private LogWrite _logWrite = LogWrite.Instance;
        private Thread _imageDrawThread = new Thread(() => { });
        private DrawingManager _drawingManager = new DrawingManager();
        private BitmapConverter _bmpConvertor = new BitmapConverter();
        public bool IsRun { get; set; } = false;

        public delegate void PostprocessCompleteDelegate(VisionProResult result);
        public event PostprocessCompleteDelegate PostprocessComplete;

        public delegate void DisplayUpdateDelegate(DisplayData displayData);
        public event DisplayUpdateDelegate DisplayUpdateEvent;

        public Postprocessor()
        {
        }

        public void Start()
        {
            try
            {
                IsRun = true;
                //_imageDrawThread = new Thread(new ThreadStart(ImageDrawProcess));
                //_imageDrawThread.Name = "VisionPro Inspection Thread";
                //_imageDrawThread.Start();
            }
            catch (Exception err)
            {
                _logWrite?.Error(err);
                Stop();
                IsRun = false;
            }
        }

        public void Stop()
        {
            try
            {
                if (!_imageDrawThread.IsAlive)
                    return;

                IsRun = false;

                if (_imageDrawThread.Join(1000))
                    _imageDrawThread.Abort();
            }
            catch (Exception err)
            {
                _logWrite?.Error(err);
            }
        }

        public void PostProcess(VPDLResult result)
        {
            IntPtr originPImage = IntPtr.Zero;
            _bmpConvertor.BitmapToInPtr(result.OriginBmp, ref originPImage);

            IntPtr overlayPImage = IntPtr.Zero;
            _bmpConvertor.BitmapToInPtr(result.OverlayBmp, ref overlayPImage);

            DisplayUpdateEvent(new DisplayData()
            { 
                IsPass = result.IsPass, 
                OriginPImage = originPImage,
                OverlayPImage = overlayPImage
            });
        }

        public void CreateOverlayBmp(VisionProResult vpResult)
        {
            int boxCount = vpResult.BoxDatas.Count;
            int barcodeCount = vpResult.GetBarcodeCount();

            // 찾은 박스가 없다면
            if (vpResult.BoxDatas.Count == 0)
            {
                IntPtr nonBoxPImage = IntPtr.Zero;
                _bmpConvertor.BitmapToInPtr(vpResult.Bmp, ref nonBoxPImage);

                DisplayData nonBoxDisplayData = new DisplayData()
                {
                    OverlayPImage = nonBoxPImage,
                    IsPass = vpResult.IsPass
                };

                // UI 업데이트
                DisplayUpdateEvent(nonBoxDisplayData);
                return;
            }

            var boxRects = new List<RectangleF>();
            var barcodeRects = new List<RectangleF>();

            // 좌표값을 통해 Rect 생성
            for (int i = 0; i < vpResult.BoxDatas.Count; i++)
            {
                CreateBoxRectangle(boxRects, vpResult.BoxDatas[i]);

                for (int j = 0; j < vpResult.BoxDatas[i].Barcodes.Count; j++)
                {
                    //CreateBarcodeRectangle(barcodeRects, vpResult.BoxDatas[i].Barcodes[j], _loadedRecipe);
                }
            }

            // TODO: Clone할 경우 내부 IntPtr은 얕은 복사로 된다고 하는데 테스트 해봐야함
            Bitmap drawBmp = vpResult.Bmp.Clone() as Bitmap;

            //_drawingManager.DrawRectangles(drawBmp, _loadedRecipe.BoxColor, _loadedRecipe.PenSize, boxRects);
            //if(barcodeRects.Count > 0)
            //    _drawingManager.DrawRectangles(drawBmp, _loadedRecipe.BarcodeColor, _loadedRecipe.PenSize, barcodeRects);

            IntPtr boxPImage = IntPtr.Zero;
            _bmpConvertor.BitmapToInPtr(drawBmp, ref boxPImage);
            DisplayData boxDisplayData = new DisplayData()
            {
                OverlayPImage = boxPImage,
                IsPass = vpResult.IsPass
            };

            // UI 업데이트
            DisplayUpdateEvent(boxDisplayData);
            //TODO: Test 해야함.
            drawBmp.Dispose();
        }

        private void CreateBoxRectangle(List<RectangleF> rectList, Box boxInfo)
        {
            Box box = boxInfo;
            PointF boxPoint = new PointF((float)box.X - ((float)box.Width / 2), (float)box.Y - ((float)box.Height / 2));
            SizeF size = new SizeF((float)box.Width, (float)box.Height);

            rectList.Add(new RectangleF(boxPoint, size));
        }

        //private void CreateBarcodeRectangle(List<RectangleF> rectList, Barcode barcodeInfo, VisionProRecipe recipe)
        //{
        //    Barcode barcode = barcodeInfo;
        //    PointF barcodePoint = new PointF((float)barcode.X - (recipe.BarcodeWidth / 2), (float)barcode.Y - (recipe.BarcodeHeight / 2));
        //    SizeF barcodesize = new SizeF(recipe.BarcodeWidth, recipe.BarcodeHeight);

        //    rectList.Add(new RectangleF(barcodePoint, barcodesize));
        //}

        public void Dispose()
        {
            try
            {
                Stop();
            }
            catch{ }
        }
    }
}
