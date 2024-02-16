using Service.Camera.Models;
using Service.Camera.Services;
using Service.ImageMerge.Models;
using Service.Logger.Services;
using Service.Postprocessing.Services;
using Service.Save.Services;
using Service.Setting.Models;
using Service.VisionPro.Models;
using Service.VisionPro.Services;
using Services.ImageMerge.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Service.MainInspection.Services
{
    public class MainInspectionManager : IMainInpectionManager
    {
        private LogWrite _logWrite = LogWrite.Instance;
        private ICameraManager _camManager;
        private IImageMergeManager _imManager;
        private IVisionProManager _vpManager;
        private IPostprocessingManager _ppManager;
        private ISaveManager _saveManager;
        private Thread _mainInspectionThread = new Thread(() => { });
        //private Dictionary<string, byte[]> _rawDataDic = new Dictionary<string, byte[]>();
        private object _rawDataFlagLock = new object();
        private AutoResetEvent _rawDataSyncEvent = new AutoResetEvent(false);
        public bool IsRun { get; set; } = false;

        public void Initialize(ICameraManager cm, IImageMergeManager imm, IVisionProManager vpm, IPostprocessingManager ppm, ISaveManager sm)
        {
            _camManager = cm;
            _imManager = imm;
            _vpManager = vpm;
            _ppManager = ppm;
            _saveManager = sm;

            foreach (var cam in _camManager.CameraDic.Values)
            {
                cam.ReceiveRawDataEnqueueComplete += OnBooleanDicUpdate;
            }
        }

        public void Run(VisionProRecipe recipe)
        {
            _saveManager.Start();
            _ppManager.Load(recipe);
            //_ppManager.Start();  
            _vpManager.RecipeLoad(recipe);
            //_vpManager.Run();
            //_imManager.Start();
            _camManager.AcqStarts();
            _camManager.GrabStarts();

            if (_mainInspectionThread.IsAlive)
            {
                IsRun = false;

                if(!_mainInspectionThread.Join(500))
                    _mainInspectionThread.Abort();
            }
                
            IsRun = true;

            _mainInspectionThread = new Thread(new ThreadStart(MainInspectionProcess));
            _mainInspectionThread.Name = "Main Inspection Thread";
            _mainInspectionThread.Start();

            _logWrite?.Info("Inspection start!!");
        }

        public void Stop()
        {
            try
            {
                
                _camManager.GrabStops();
                _camManager.AcqStops();
                _saveManager.Stop();
                if (!_mainInspectionThread.IsAlive)
                    return;

                IsRun = false;

                if (_mainInspectionThread.Join(2000))
                    _mainInspectionThread.Abort();

                ResetRawDataQueue(_camManager);
            }
            catch (Exception err)
            {
                _logWrite?.Error(err);
            }
        }

        public void Dispose()
        {
            foreach (var cam in _camManager.CameraDic.Values)
            {
                cam.ReceiveRawDataEnqueueComplete -= OnBooleanDicUpdate;
            }
            //_rawDataDic.Clear();
        }

        private void OnBooleanDicUpdate(object sender, string e)
        {
            lock (_rawDataFlagLock)
            {
                _camManager.CameraDic[e].IsEnqueue = true;
                bool flag = _camManager.CameraDic.Values.Any(val => val.IsEnqueue = false);
                if (!flag) _rawDataSyncEvent.Set();
            }
        }

        private void MainInspectionProcess()
        {
            ICamera standardCam = _camManager.CameraDic.Values.First();
            BitmapData bmpData = CreateBitmapData(standardCam.CamConfig);
            Stopwatch sw = new Stopwatch();


            while (IsRun)
            {
                //if (!_rawDataSyncEvent.WaitOne(2000))
                //{
                //    //TODO : 언딜리버드 발생하는 것을 고려하여 IO 신호 확인해서 트리거 관련 신호가 켜져있다면 Retry
                //    continue;
                //}
                //TODO : Test
                bool flag = true;
                while (flag)
                {
                    bool rawDataFlag = true;
                    foreach (var cam in _camManager.CameraDic.Values)
                    {
                        if (cam.RawDatas.Count == 0)
                        {
                            rawDataFlag = false;
                        }
                    }

                    if (rawDataFlag) flag = false;
                    Thread.Sleep(10);
                }

                _camManager.CameraDic["Cam1"].RawDatas.TryDequeue(out var cam1RawData);
                _camManager.CameraDic["Cam2"].RawDatas.TryDequeue(out var cam2RawData);
                _camManager.CameraDic["Cam3"].RawDatas.TryDequeue(out var cam3RawData);
                _camManager.CameraDic["Cam4"].RawDatas.TryDequeue(out var cam4RawData);
                ResetRawDataQueue(_camManager);

                // Processing Time : 200 ~ 250ms 예상
                sw.Restart();
                MergeBitmap topMergeBmp = _imManager.CreateMergeBitmap(bmpData, standardCam.CamConfig.Buffersize * 2, cam2RawData, cam1RawData);
                MergeBitmap botMergeBmp = _imManager.CreateMergeBitmap(bmpData, standardCam.CamConfig.Buffersize * 2, cam4RawData, cam3RawData);
                sw.Stop();
                _logWrite?.Info($"Image merge complete!!, Processing time : {sw.ElapsedMilliseconds}", false, false);

                // Processing Time : 300 ~ 330ms 예상
                sw.Restart();
                Task<VisionProResult> topInspectionTask = Task.Run(() => _vpManager.InspectorDic["Top"].BCRInspection(topMergeBmp));
                Task<VisionProResult> botInspectionTask = Task.Run(() => _vpManager.InspectorDic["Bottom"].BCRInspection(botMergeBmp));

                // 검사가 완료될 때까지 대기
                topInspectionTask.Wait();
                botInspectionTask.Wait();

                DateTime dt = DateTime.Now;
                topInspectionTask.Result.InspectionTime = dt;
                botInspectionTask.Result.InspectionTime = dt;
                sw.Stop();

                _logWrite?.Info($"VisionPro inspection complete!!, Processing time : {sw.ElapsedMilliseconds}", false, false);

                _logWrite?.Info($"VisionPro inspection result info.{Environment.NewLine}" +
                    $"Top box count : {topInspectionTask.Result.BoxDatas.Count}{Environment.NewLine}" +
                    $"Top barcode count : {topInspectionTask.Result.GetBarcodeCount()}{Environment.NewLine}" +
                    $"Bottom box count :  {botInspectionTask.Result.BoxDatas.Count}{Environment.NewLine}" +
                    $"Bottom barcode count : {botInspectionTask.Result.GetBarcodeCount()}");

                topMergeBmp.Dispose();
                botMergeBmp.Dispose();

                // ProcessingTime : 150 ~ 200ms 예상
                //오버레이 이미지 생성 후 UI 업데이트까지 진행
                sw.Restart();
                _ppManager.ProcessorDic["Top"].CreateOverlayBmp(topInspectionTask.Result);
                _ppManager.ProcessorDic["Bottom"].CreateOverlayBmp(botInspectionTask.Result);
                sw.Stop();
                _logWrite?.Info($"Postprocessing complete!!, Processing time : {sw.ElapsedMilliseconds}", false, false);

                // 결과가 False 라면
                if (!topInspectionTask.Result.IsPass || !botInspectionTask.Result.IsPass)
                {
                    // TODO : Retry 기능 구현
                }

                // 결과가 True라면
                if (topInspectionTask.Result.IsPass && botInspectionTask.Result.IsPass)
                {
                    if(_saveManager.SaveQueue.Count > 20)
                    {
                        topInspectionTask.Result.Dispose();
                        botInspectionTask.Result.Dispose();
                        _logWrite?.Info("Save queue is too stacked..");
                    }
                    else
                    {
                        _saveManager.SaveQueue.Enqueue(topInspectionTask.Result);
                        _saveManager.SaveQueue.Enqueue(botInspectionTask.Result);
                    }
                    //TODO : 상위서버 데이터 전송
                }
            }
        }

        /// <summary>
        /// rawData Flag 딕셔너리, rawData 큐 초기화 진행
        /// </summary>
        /// <param name="cameraManager"></param>        
        private void ResetRawDataQueue(ICameraManager cameraManager)
        {
            lock (_rawDataFlagLock)
            {
                foreach (var key in cameraManager.CameraDic.Keys)
                {
                    _camManager.CameraDic[key].IsEnqueue = false;
                }
            }

            foreach (var cam in cameraManager.CameraDic.Values)
            {
                for (int i = 0; i < cam.RawDatas.Count; i++)
                {
                    cam.RawDatas.TryDequeue(out _);
                }
            }
        }

        /// <summary>
        /// rawData 일괄 Dequeue 진행
        /// </summary>
        /// <param name="camManager"></param>
        /// <param name="rawDataDic"></param>
        private void RawDataDequeue(ICameraManager camManager, Dictionary<string, byte[]> rawDataDic)
        {
            foreach (var keyVal in camManager.CameraDic)
            {
                keyVal.Value.RawDatas.TryDequeue(out byte[] rawData);
                rawDataDic[keyVal.Key] = rawData;
            }
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
