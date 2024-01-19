using Service.Camera.Services.ConvertService;
using Service.Drawing.Services;
using Service.Logger.Services;
using Service.Postprocessing.Models;
using Service.Setting.Models;
using Service.VisionPro.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Media.Imaging;

namespace Service.Postprocessing.Services
{
    public class Postprocessor : IDisposable
    {
        private LogWrite _logWrite = LogWrite.Instance;
        private ConcurrentQueue<VisionProResult> _visionProResultQueue;
        private Thread _imageDrawThread = new Thread(() => { });
        private DrawingManager _drawingManager = new DrawingManager();
        private BitmapConverter _bmpConvertor = new BitmapConverter();
        private VisionProRecipe _loadedRecipe;
        public bool IsRun { get; set; } = false;

        public delegate void PostprocessCompleteDelegate(PostprocessingResult result);
        public event PostprocessCompleteDelegate PostprocessComplete;

        public delegate void DisplayUpdateDelegate(DisplayData displayData);
        public event DisplayUpdateDelegate DisplayUpdateEvent;

        public Postprocessor(ConcurrentQueue<VisionProResult> vpResultQueue)
        {
            _visionProResultQueue = vpResultQueue;
        }

        public void RecipeLoad(VisionProRecipe recipe)
        {
            _loadedRecipe = recipe;
        }

        public void Start()
        {
            try
            {
                IsRun = true;
                //TODO :Test
                _imageDrawThread = new Thread(new ThreadStart(ImageDrawProcess2));
                _imageDrawThread.Name = "VisionPro Inspection Thread";
                _imageDrawThread.Start();
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

        private void ImageDrawProcess()
        {
            //Tact 40ms
            while (IsRun)
            {
                VisionProResult vpResult = null;
                if (!_visionProResultQueue.TryDequeue(out vpResult))
                {
                    Thread.Sleep(10);
                    continue;
                }

                var ppResult = new PostprocessingResult(vpResult);

                var boxRects = new List<RectangleF>();
                var barcodeRects = new List<RectangleF>();
                if (vpResult.BoxDatas.Count > 0) 
                {
                    for (int i = 0; i < vpResult.BoxDatas.Count; i++)
                    {
                        CreateBoxRectangle(boxRects, vpResult.BoxDatas[i]);

                        if (vpResult.BoxDatas[i].Barcodes != null && vpResult.BoxDatas[i].Barcodes.Count > 0)
                        {
                            for (int j = 0; j < vpResult.BoxDatas[i].Barcodes.Count; j++)
                            {
                                CreateBarcodeRectangle(barcodeRects, vpResult.BoxDatas[i].Barcodes[j], _loadedRecipe);
                            }
                        }
                    }
                }

                if (boxRects.Count > 0)
                {
                    Bitmap drawBmp = null;

                    drawBmp = _drawingManager.DrawRectangles(vpResult.OriginBmp.Clone() as Bitmap, _loadedRecipe.BoxColor, _loadedRecipe.PenSize, boxRects);

                    if (barcodeRects.Count > 0)
                        ppResult.OverlayBmp = _drawingManager.DrawRectangles(drawBmp, _loadedRecipe.BarcodeColor, _loadedRecipe.PenSize, barcodeRects);
                    else
                        ppResult.OverlayBmp = drawBmp;
                }
                else
                    ppResult.OverlayBmp = vpResult.OriginBmp.Clone() as Bitmap;
                

                // UI에 뿌려줄 데이터 생성
                int boxCount = vpResult.BoxDatas.Count;
                int barcodeCount = vpResult.GetBarcodeCount();

                BitmapImage bmpImage = _bmpConvertor.BitmapToBitmapImage(ppResult.OverlayBmp);

                DisplayData displayData = new DisplayData()
                {
                    BmpImage = bmpImage,
                    BoxCount = boxCount,
                    BarcodeCount = barcodeCount
                };
                DisplayUpdateEvent(displayData);

                // SaveManager에 뿌려줌
                if (vpResult.IsPass)
                    PostprocessComplete(ppResult);
                else
                    ppResult.Dispose();

                Thread.Sleep(10);
            }
        }
        //TODO :test
        private void ImageDrawProcess2()
        {
            //Tact 40ms
            while (IsRun)
            {
                VisionProResult vpResult = null;
                if (!_visionProResultQueue.TryDequeue(out vpResult))
                {
                    Thread.Sleep(10);
                    continue;
                }

                var ppResult = new PostprocessingResult(vpResult);

                var boxRects = new List<RectangleF>();
                var barcodeRects = new List<RectangleF>();
                if (vpResult.BoxDatas.Count > 0)
                {
                    for (int i = 0; i < vpResult.BoxDatas.Count; i++)
                    {
                        CreateBoxRectangle(boxRects, vpResult.BoxDatas[i]);
                    }
                }

                if (boxRects.Count > 0)
                    ppResult.OverlayBmp = _drawingManager.DrawRectangles(vpResult.OriginBmp.Clone() as Bitmap, _loadedRecipe.BoxColor, _loadedRecipe.PenSize, boxRects);
                else
                    ppResult.OverlayBmp = vpResult.OriginBmp.Clone() as Bitmap;


                // UI에 뿌려줄 데이터 생성
                int boxCount = vpResult.BoxDatas.Count;
                int barcodeCount = 0;

                BitmapImage bmpImage = _bmpConvertor.BitmapToBitmapImage(ppResult.OverlayBmp);

                DisplayData displayData = new DisplayData()
                {
                    BmpImage = bmpImage,
                    BoxCount = boxCount,
                    BarcodeCount = barcodeCount
                };
                DisplayUpdateEvent(displayData);

                // SaveManager에 뿌려줌
                if (vpResult.IsPass)
                    PostprocessComplete(ppResult);
                else
                    ppResult.Dispose();

                Thread.Sleep(10);
            }
        }

        private void CreateBoxRectangle(List<RectangleF> rectList, Box boxInfo)
        {
            Box box = boxInfo;
            PointF boxPoint = new PointF((float)box.X - ((float)box.Width / 2), (float)box.Y - ((float)box.Height / 2));
            SizeF size = new SizeF((float)box.Width, (float)box.Height);

            rectList.Add(new RectangleF(boxPoint, size));
        }

        private void CreateBarcodeRectangle(List<RectangleF> rectList, Barcode barcodeInfo, VisionProRecipe recipe)
        {
            Barcode barcode = barcodeInfo;
            PointF barcodePoint = new PointF((float)barcode.X - (recipe.BarcodeWidth / 2) , (float)barcode.Y - (recipe.BarcodeHeight / 2));
            SizeF barcodesize = new SizeF(recipe.BarcodeWidth, recipe.BarcodeHeight);

            rectList.Add(new RectangleF(barcodePoint, barcodesize));
        }

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
