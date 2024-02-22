using Service.Camera.Services.ConvertService;
using Service.Drawing.Services;
using Service.Logger.Services;
using Service.Postprocessing.Models;
using Service.VisionPro.Models;
using System;
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

        public void PostProcess(VisionProResult result)
        {
            //TODO : 작성해야함
            DisplayUpdateEvent(new DisplayData()
            {
                IsPass = result.IsPass,
            });
        }



        //private void CreateBoxRectangle(List<RectangleF> rectList, Box boxInfo)
        //{
        //    Box box = boxInfo;
        //    PointF boxPoint = new PointF((float)box.X - ((float)box.Width / 2), (float)box.Y - ((float)box.Height / 2));
        //    SizeF size = new SizeF((float)box.Width, (float)box.Height);

        //    rectList.Add(new RectangleF(boxPoint, size));
        //}

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
