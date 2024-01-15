using Service.Camera.Models;
using Service.Camera.Services.ConvertService;
using Service.ImageMerge.Models;
using Service.Logger.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services.ImageMerge.Services
{
    public class ImageMergeManager : IImageMergeManager
    {
        private LogWrite _logWrite = LogWrite.Instance;
        private ICameraManager _cameraManager;
        private Thread _mergeThread = new Thread(() => { });
        private BitmapConverter _bmpConverter = BitmapConverter.Instance;
        private Dictionary<string, byte[]> _rawDataDic = new Dictionary<string, byte[]>();

        public ConcurrentQueue<MergeBitmap> TopMergeBitmapQueue { get; set; } = new ConcurrentQueue<MergeBitmap>();
        public ConcurrentQueue<MergeBitmap> BotMergeBitmapQueue { get; set; } = new ConcurrentQueue<MergeBitmap>();
        public bool IsRun { get; set; } = false;

        public ImageMergeManager()
        {
        }

        public void Initialize(ICameraManager cm)
        {
            _cameraManager = cm;

            foreach (var keyVal in _cameraManager.CameraDic)
            {
                _rawDataDic.Add(keyVal.Key, null);
            }
        }

        public void Start()
        {
            try
            {
                if (TopMergeBitmapQueue.Count > 0)
                {
                    for (int i = 0; i < TopMergeBitmapQueue.Count; i++)
                    {
                        MergeBitmap mergeBmp;
                        TopMergeBitmapQueue.TryDequeue(out mergeBmp);
                        mergeBmp.Dispose();
                    }
                }

                if (BotMergeBitmapQueue.Count > 0)
                {
                    for (int i = 0; i < BotMergeBitmapQueue.Count; i++)
                    {
                        MergeBitmap mergeBmp;
                        BotMergeBitmapQueue.TryDequeue(out mergeBmp);
                        mergeBmp.Dispose();
                    }
                }

                IsRun = true;

                _mergeThread = new Thread(new ThreadStart(ImageMergeProcess));
                _mergeThread.Name = "Image Merge Thread";
                _mergeThread.Start();

                
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
                if (!_mergeThread.IsAlive)
                    return;

                IsRun = false;

                if (_mergeThread.Join(1000))
                    _mergeThread.Abort();

                if(TopMergeBitmapQueue.Count > 0)
                {
                    for (int i = 0; i < TopMergeBitmapQueue.Count; i++)
                    {
                        MergeBitmap mergeBmp;
                        TopMergeBitmapQueue.TryDequeue(out mergeBmp);
                        mergeBmp.Dispose();
                    }
                }

                if (BotMergeBitmapQueue.Count > 0)
                {
                    for (int i = 0; i < BotMergeBitmapQueue.Count; i++)
                    {
                        MergeBitmap mergeBmp;
                        BotMergeBitmapQueue.TryDequeue(out mergeBmp);
                        mergeBmp.Dispose();
                    }
                }
            }
            catch (Exception err)
            {
                _logWrite?.Error(err);
            }
        }

        public void Dispose()
        {
            try
            {
                Stop();
            }
            catch
            {
            }
        }

        private void ImageMergeProcess()
        {
            try
            {
                int errCount = 0;
                ICamera standardCam = _cameraManager.CameraDic.Values.First();

                while (IsRun)
                {
                    try
                    {
                        bool flag = true;
                        foreach (var cam in _cameraManager.CameraDic)
                        {
                            if (cam.Value.RawDatas.Count > 0)
                                flag = false;
                        }

                        if (!flag)
                        {
                            Thread.Sleep(10);
                            continue;
                        }

                        foreach (var keyVal in _cameraManager.CameraDic)
                        {
                            byte[] rawData = null;
                            keyVal.Value.RawDatas.TryDequeue(out rawData);
                            _rawDataDic[keyVal.Key] = rawData;
                        }

                        BitmapData bmpData = CreateBitmapData(standardCam.CamConfig);

                        // Top Raw Data 병합
                        byte[] topRawData = new byte[standardCam.CamConfig.Buffersize * 2];
                        MergeRawData(topRawData, _rawDataDic["Cam2"], _rawDataDic["Cam1"]);

                        // Raw Data => InPtr => Bitmap 순으로 생성
                        MergeBitmap topMergeBmp = RawDataToMergeBitmap(topRawData, bmpData);
                        topMergeBmp.Bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        TopMergeBitmapQueue.Enqueue(topMergeBmp);

                        // Bottom Raw Data 병합
                        byte[] botRawData = new byte[standardCam.CamConfig.Buffersize * 2];
                        MergeRawData(botRawData, _rawDataDic["Cam4"], _rawDataDic["Cam3"]);

                        // Raw Data => InPtr => Bitmap 순으로 생성
                        MergeBitmap botMergeBmp = RawDataToMergeBitmap(botRawData, bmpData);
                        botMergeBmp.Bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        BotMergeBitmapQueue.Enqueue(botMergeBmp);

                        Thread.Sleep(10);
                        //TODO : 여기 까지 최적화 완료함. 비전프로 최적화, 검사 항목 추가, DB 저장 기능 추가 해야함
                    }
                    catch (Exception err)
                    {
                        if(errCount >= 10)
                        {
                            _logWrite?.Error(err);
                            break;
                        }
                        _logWrite?.Error(err, false, true);
                        errCount++;
                    }
                }
            }
            catch (Exception err)
            {
                _logWrite?.Error(err);
            }
            
        }
        /// <summary>
        /// byte[] 상부 하부 순으로 병합함 (-90도 기울어서 이미지가 들어오기 때문)
        /// </summary>
        /// <param name="mergeRawData"></param>
        /// <param name="topRawData"></param>
        /// <param name="botRawData"></param>
        private void MergeRawData(byte[] mergeRawData, byte[] topRawData, byte[] botRawData)
        {
            Buffer.BlockCopy(topRawData, 0, mergeRawData, 0, topRawData.Length);
            Buffer.BlockCopy(botRawData, 0, mergeRawData, topRawData.Length, botRawData.Length);
        }

        /// <summary>
        /// Raw Data => InPtr => Bitmap 순으로 생성
        /// </summary>
        /// <param name="mergeRawData"></param>
        /// <param name="bmpData"></param>
        /// <returns></returns>
        private MergeBitmap RawDataToMergeBitmap(byte[] mergeRawData, BitmapData bmpData)
        {
            MergeBitmap mergeBmp = new MergeBitmap();

            mergeBmp.PImage = Marshal.AllocHGlobal(mergeRawData.Length);
            Marshal.Copy(mergeRawData, 0, mergeBmp.PImage, mergeRawData.Length);
            mergeBmp.Bmp = _bmpConverter.IntPtrToBitmap(bmpData, mergeBmp.PImage);

            return mergeBmp;
        }

        private BitmapData CreateBitmapData(CameraConfig camConfig)
        {
            BitmapData bmpData = new BitmapData()
            {
                Stride = camConfig.Stride,
                Height = camConfig.Height * 2,
                Width = camConfig.Width,
                PixelFormat = camConfig.BitmapPixelFormat
            };

            return bmpData;
        }
    }
}
