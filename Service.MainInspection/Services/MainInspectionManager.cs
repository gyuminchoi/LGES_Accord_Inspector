using Service.Camera.Models;
using Service.Logger.Services;
using Service.Postprocessing.Models;
using Service.Postprocessing.Services;
using Service.VisionPro.Models;
using Service.VisionPro.Services;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Service.MainInspection.Services
{
    public class MainInspectionManager : IMainInpectionManager
    {
        private LogWrite _logWrite = LogWrite.Instance;
        private ICameraManager _camManager;
        private IVisionProManager _vpManager;
        private IPostprocessingManager _ppManager;
        private Thread _mainInspectionThread = new Thread(() => { });
        private AutoResetEvent _receiveDataSyncEvent = new AutoResetEvent(false);

        public bool IsRun { get; set; } = false;

        public void Initialize(ICameraManager cm, IVisionProManager vpm, IPostprocessingManager ppm)
        {
            _camManager = cm;
            _vpManager = vpm;
            _ppManager = ppm;

            foreach (var cam in _camManager.CameraDic.Values)
            {
                cam.ReceiveImageDataEnqueueComplete += OnBooleanDicUpdate;
            }
        }

        public void Run()
        {
            try
            {
                _camManager.AcqStarts();
                _camManager.GrabStarts();

                if (_mainInspectionThread.IsAlive)
                {
                    IsRun = false;

                    if (!_mainInspectionThread.Join(500))
                        _mainInspectionThread.Abort();
                }

                IsRun = true;

                _mainInspectionThread = new Thread(new ThreadStart(MainInspectionProcess));
                _mainInspectionThread.Name = "Main Inspection Thread";
                _mainInspectionThread.Start();

                _logWrite?.Info("Inspection start!!");
            }
            catch (Exception err)
            {
                _logWrite?.Error(err);
            }
        }

        public void Stop()
        {
            try
            {
                _camManager.GrabStops();
                _camManager.AcqStops();
                if (!_mainInspectionThread.IsAlive)
                    return;

                IsRun = false;

                if (_mainInspectionThread.Join(2000))
                    _mainInspectionThread.Abort();

                ResetReceiveImageDataQueue(_camManager);
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
                cam.ReceiveImageDataEnqueueComplete -= OnBooleanDicUpdate;
            }
        }

        private void OnBooleanDicUpdate(object sender, string e)
        {
            bool flag = true;
            foreach (var cam in _camManager.CameraDic.Values)
            {
                if(cam.ReceiveImageDatas.Count == 0)
                {
                    flag = false;
                }
            }

            if(flag) _receiveDataSyncEvent.Set();
        }

        private void MainInspectionProcess()
        {
            ICamera standardCam = _camManager.CameraDic.Values.First();
            BitmapData bmpData = CreateBitmapData(standardCam.CamConfig);

            while (IsRun)
            {
                if (!_receiveDataSyncEvent.WaitOne(3000))
                {
                    continue;
                }

                _camManager.CameraDic["SWIR"].ReceiveImageDatas.TryDequeue(out Bitmap swirBmp);
                _camManager.CameraDic["Standard"].ReceiveImageDatas.TryDequeue(out Bitmap stdBmp);

                ResetReceiveImageDataQueue(_camManager);

                Task<VisionProResult> swirInspection = Task.Run(() => _vpManager.InspectorDic["SWIR"].VisionProInspection(swirBmp));
                Task<VisionProResult> stdInspection = Task.Run(() => _vpManager.InspectorDic["Standard"].VisionProInspection(stdBmp));

                // 검사가 완료될 때까지 대기
                swirInspection.Wait();
                stdInspection.Wait();

                // 후처리 후 UI 업데이트까지 진행
                Task swirPostProcess = Task.Run(() => _ppManager.ProcessorDic["SWIR"].PostProcess(swirInspection.Result));
                Task stdPostProcess = Task.Run(() => _ppManager.ProcessorDic["Standard"].PostProcess(stdInspection.Result));

                swirPostProcess.Wait();
                stdPostProcess.Wait();

                swirInspection.Result.Dispose();
                stdInspection.Result.Dispose();
            }
        }

        /// <summary>
        /// rawData Flag 딕셔너리, rawData 큐 초기화 진행
        /// </summary>
        /// <param name="cameraManager"></param>        
        private void ResetReceiveImageDataQueue(ICameraManager cameraManager)
        {
            //TODO : 메모리 누수 있는 지 확인 해야함
            foreach (var cam in cameraManager.CameraDic.Values)
            {
                for (int i = 0; i < cam.ReceiveImageDatas.Count; i++)
                {
                    cam.ReceiveImageDatas.TryDequeue(out _);
                }
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
