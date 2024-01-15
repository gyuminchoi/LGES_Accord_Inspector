using Service.Drawing.Services;
using Service.Logger.Services;
using Service.VisionPro.Models;
using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Poseprocessing.Services
{
    public class Postprocessor
    {
        private LogWrite _logWrite = LogWrite.Instance;
        private ConcurrentQueue<VisionProResult> _visionProResultQueue;
        private Thread _imageDrawThread = new Thread(() => { });
        private DrawingManager _drawingManager = new DrawingManager();

        public bool IsRun { get; set; } = false;


        public Postprocessor(ConcurrentQueue<VisionProResult> vpResultQueue)
        {
            _visionProResultQueue = vpResultQueue;
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
            if (!_imageDrawThread.IsAlive)
                return;

            IsRun = false;

            if (_imageDrawThread.Join(1000))
                _imageDrawThread.Abort();
        }

        private void ImageDrawProcess()
        {
            VisionProResult result = null;
            while (IsRun)
            {
                if(!_visionProResultQueue.TryDequeue(out result))
                {
                    Thread.Sleep(10);
                    continue;
                }

                // TODO : 색, 펜 사이즈 셋팅으로 빼야함
                List<RectangleF> rectList = new List<RectangleF>();
                for (int i = 0; i < result.BoxDatas.Count; i++)
                {
                    // TODO : Box Rect 생성
                    CreateBoxRectangle(rectList, result.BoxDatas[i]);

                    for (int j = 0; j < result.BoxDatas[i].Barcodes.Count; j++)
                    {
                        // TODO : Barcode Rect 생성
                        CreateBarcodeRectangle(rectList, result.BoxDatas[i].Barcodes[j]);
                    }
                }
                // TODO : Overlay Bitmap 생성
                result.OverlayBmp = _drawingManager.DrawRectangles(result.OriginBmp.Clone() as Bitmap, Color.Red, 10, rectList);

                // TODO : 완성된 오버레이 이미지, VisionProResult 객체 구독한 ViewModel에 전달
                
            }
        }

        private void CreateBoxRectangle(List<RectangleF> rectList, Box boxInfo)
        {
            Box box = boxInfo;
            PointF boxPoint = new PointF((float)box.X - ((float)box.Width / 2), (float)box.Y - ((float)box.Height / 2));
            SizeF size = new SizeF((float)box.Width, (float)box.Height);

            rectList.Add(new RectangleF(boxPoint, size));
        }

        // TODO : 세팅에서 바코드 Border 사이즈 받아와서 (Point - (barcode.Width 또는 Height / 2))해야함
        private void CreateBarcodeRectangle(List<RectangleF> rectList, Barcode barcodeInfo)
        {
            Barcode barcode = barcodeInfo;
            PointF barcodePoint = new PointF((float)barcode.X, (float)barcode.Y);
            SizeF barcodesize = new SizeF(100, 100);

            rectList.Add(new RectangleF(barcodePoint, barcodesize));
        }
    }
}
