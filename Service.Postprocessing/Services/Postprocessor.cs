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

namespace Service.Postprocessing.Services
{
    public class Postprocessor : IDisposable
    {
        private LogWrite _logWrite = LogWrite.Instance;
        private ConcurrentQueue<VisionProResult> _visionProResultQueue;
        private Thread _imageDrawThread = new Thread(() => { });
        private DrawingManager _drawingManager = new DrawingManager();
        private VisionProRecipe _loadedRecipe;
        public bool IsRun { get; set; } = false;

        public delegate void PostprocessCompleteDelegate(PostprocessingResult result);
        public event PostprocessCompleteDelegate PostprocessComplete;

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

                _imageDrawThread = new Thread(new ThreadStart(ImageDrawProcess));
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
            VisionProResult visionProResult = null;
            //Tact 30ms
            while (IsRun)
            {
                if(!_visionProResultQueue.TryDequeue(out visionProResult))
                {
                    Thread.Sleep(10);
                    continue;
                }
                var result = new PostprocessingResult();
                result.VisionProResult = visionProResult;

                List<RectangleF> barcodeRectList = new List<RectangleF>();
                List<RectangleF> boxRectList = new List<RectangleF>();
                for (int i = 0; i < visionProResult.BoxDatas.Count; i++)
                {
                    CreateBoxRectangle(boxRectList, visionProResult.BoxDatas[i]);

                    for (int j = 0; j < visionProResult.BoxDatas[i].Barcodes.Count; j++)
                    {
                        CreateBarcodeRectangle(barcodeRectList, visionProResult.BoxDatas[i].Barcodes[j], _loadedRecipe);
                    }
                }
                Bitmap boxBmp = _drawingManager.DrawRectangles(visionProResult.OriginBmp.Clone() as Bitmap, _loadedRecipe.BoxColor, _loadedRecipe.PenSize, boxRectList);
                result.OverlayBmp = _drawingManager.DrawRectangles(boxBmp, _loadedRecipe.BarcodeColor, _loadedRecipe.PenSize, barcodeRectList);

                PostprocessComplete(result);
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
