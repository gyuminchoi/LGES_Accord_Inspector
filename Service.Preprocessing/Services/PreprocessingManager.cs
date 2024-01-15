//using Service.Camera.Models;
//using Service.Camera.Services.ConvertService;
//using Service.Logger.Services;
//using Service.Pattern;
//using Service.Preprocessing.Models;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Threading;

//namespace Service.Preprocessing.Services
//{
//    /// <summary>
//    /// ICameraManager, VisionPro Manager가 초기화 되어야함
//    /// </summary>
//    public class PreprocessingManager : IPreprocessingManager
//    {
//        private ICameraManager _camManager;
//        private BitmapConverter _bmpConverter = BitmapConverter.Instance;
//        private LogWrite _logWrite = LogWrite.Instance; 
//        private Thread _mergeThread = new Thread(() => { });
//        private object _receiveLock = new object();

//        public bool IsRun { get; set; } = false;
//        public Dictionary<string, ReceivedBitmap> ReceivedBitmapDic { get; set; } = new Dictionary<string, ReceivedBitmap>();
//        public ConcurrentQueue<Bitmap> TopMergeBitmapQueue { get; set; } = new ConcurrentQueue<Bitmap>();
//        public ConcurrentQueue<Bitmap> BotMergeBitmapQueue { get; set; } = new ConcurrentQueue<Bitmap>();

//        public PreprocessingManager() { }

//        public void Initialize(ICameraManager cm)
//        {
//            try
//            {
//                _camManager = cm;

//                foreach (var item in _camManager.CameraDic)
//                {
//                    ReceivedBitmapDic.Add(item.Key, new ReceivedBitmap());
//                }

//                foreach (var cam in _camManager.CameraDic.Values)
//                {
//                    cam.ReceiveImageDataEnqueueComplete += OnReceiveImage;
//                }
//            }
//            catch (Exception err)
//            {
//                _logWrite?.Error(err);
//                Dispose();
//            }
//        }

//        public void Start() 
//        {
//            try
//            {
//                IsRun = true;

//                _mergeThread = new Thread(new ThreadStart(ImageMergeProcess));
//                _mergeThread.Name = "Preprocessing Merge Thread";
//                _mergeThread.Start();
//            }
//            catch (Exception err)
//            {
//                _logWrite?.Error(err);
//                Stop();
//                IsRun = false;
//            }
//        }

//        public void Stop()
//        {
//            try
//            {
//                if (!_mergeThread.IsAlive)
//                    return;

//                IsRun = false;

//                if (_mergeThread.Join(1000))
//                    _mergeThread.Abort();
//            }
//            catch (Exception err)
//            {
//                _logWrite?.Error(err);
//            }
//         }

//        private void ImageMergeProcess()
//        {
//            int errCount = 0;
//            try
//            {
//                while (IsRun)
//                {
//                    try
//                    {

//                        bool flag = true;
//                        foreach (var item in ReceivedBitmapDic.Values)
//                        {
//                            if (!item.IsReceive) flag = false;
//                        }

//                        if (!flag)
//                        {
//                            Thread.Sleep(5);
//                            continue;
//                        }

//                        foreach (var item in _camManager.CameraDic)
//                        {
//                            item.Value.ReceiveImageDatas.TryDequeue(out Bitmap bmp);
//                            lock (_receiveLock)
//                            {
//                                ReceivedBitmapDic[item.Key].BMP = bmp;
//                                ReceivedBitmapDic[item.Key].IsReceive = false;
//                            }
//                        }

//                        if (ReceivedBitmapDic.ContainsKey("Cam1") && ReceivedBitmapDic.ContainsKey("Cam2"))
//                        {
//                            Bitmap bmp = BitmapMerge(ReceivedBitmapDic["Cam1"].BMP, ReceivedBitmapDic["Cam2"].BMP);
//                            TopMergeBitmapQueue.Enqueue(bmp);
//                        }

//                        if (ReceivedBitmapDic.ContainsKey("Cam3") && ReceivedBitmapDic.ContainsKey("Cam4"))
//                        {
//                            Bitmap bmp = BitmapMerge(ReceivedBitmapDic["Cam3"].BMP, ReceivedBitmapDic["Cam4"].BMP);
//                            BotMergeBitmapQueue.Enqueue(bmp);
//                        }

//                        foreach (var item in ReceivedBitmapDic.Values)
//                        {
//                            item.BMP.Dispose();
//                        }
//                        errCount = 0;
//                        Thread.Sleep(5);
//                    }
//                    catch (Exception err)
//                    {
//                        _logWrite?.Error(err, false, true);
//                        if (errCount > 100)
//                        {
//                            _logWrite?.Error(err);
//                            break;
//                        }
//                        errCount++;
//                    }
//                }
//            }
//            catch (Exception err)
//            {
//                _logWrite.Error(err);
//            }
            
//        }

//        private Bitmap BitmapMerge(Bitmap bmpLeft, Bitmap bmpRight)
//        {
//            int width = bmpLeft.Width + bmpRight.Width;
//            int height = bmpLeft.Height >= bmpRight.Height ? bmpLeft.Height : bmpRight.Height;

//            Bitmap combineBmp = new Bitmap(width, height);

//            using(Graphics g = Graphics.FromImage(combineBmp))
//            {
//                g.DrawImage(bmpLeft, 0, 0);
//                g.DrawImage(bmpRight, bmpLeft.Width, 0);
//            }

//            Bitmap bit24Bmp = _bmpConverter.ConvertTo24bpp(combineBmp);
//            Bitmap bit8Bmp = _bmpConverter.Get8GrayBitmap(bit24Bmp);

//            combineBmp.Dispose();
//            bit24Bmp.Dispose();

//            return bit8Bmp;
//        }

//        private void OnReceiveImage(object sender, string camID)
//        {
//            lock (_receiveLock) { ReceivedBitmapDic[camID].IsReceive = true; }
//        }

//        public void Dispose()
//        {
//            ReceivedBitmapDic.Clear();

//            foreach (var cam in _camManager.CameraDic.Values)
//            {
//                cam.ReceiveImageDataEnqueueComplete -= OnReceiveImage;
//            }
//        }
//    }
//}
