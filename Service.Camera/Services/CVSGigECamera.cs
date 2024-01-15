using Crevis.VirtualFG40Library;
using Service.Camera.Models;
using Service.Camera.Services.ConvertService;
using Service.CustomException.Models.ErrorTypes;
using Service.CustomException.Services.ErrorService.HandledExceptions;
using Service.Logger.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Camera.Services
{
    public class CVSGigECamera :  ICamera
    {
        private bool _isDisposing = false;
        private VirtualFG40Library _vfg;
        private GCHandle _gcHandle;
        private GCHandle _gcHandleEvent;
        private BitmapConverter _bmpConverter = BitmapConverter.Instance;
        private LogWrite _logWrite = LogWrite.Instance;
        private object _rawDatasLock = new object();
        //private Thread _imgDataThread = new Thread(new ThreadStart(() => { }));
        private Thread _reconnectThread = new Thread(new ThreadStart(() => { }));
        private AutoResetEvent _imgCallbackOnEvent = new AutoResetEvent(false);
        private bool _isDoingStop = false;
        private object _stoppingLock = new object();
        private bool _isSWTrigThread = false;
        private Thread _swTrigThread = new Thread(() => { });
        public CameraConfig CamConfig { get; set; }
        public ConcurrentQueue<byte[]> RawDatas { get; set; } = new ConcurrentQueue<byte[]>();
        //public ConcurrentQueue<Bitmap> ReceiveImageDatas { get; set; } = new ConcurrentQueue<Bitmap>();
        public int MaxEnqueueCount { get; set; } = 1000;

        //public event EventHandler<string> ReceiveImageDataEnqueueComplete;
        public event EventHandler<string> ReceiveRawDataEnqueueComplete;
        public CVSGigECamera(VirtualFG40Library vfg, uint camIndex, string userID, string modelName, string serialNumber, string deviceVersion)
        {
            _vfg = vfg;
            CamConfig = new CameraConfig()
            {
                CamIndex = camIndex,
                CameraType = ECameraType.CVSGigECamera,
                UserID = userID,
                ModelName = modelName,
                SerialNumber = serialNumber,
                DeviceVersion = deviceVersion,
            };
        }

        ~CVSGigECamera() => Dispose();

        public void Open(bool isReConnect = false)
        {
            _logWrite?.Info($"({CamConfig.UserID}) Crevis GigE Camera. Open.");

            if (CamConfig.IsOpen) return;

            var hDevice = -1;
            int status = _vfg.OpenDevice(CamConfig.CamIndex, ref hDevice, true);
            if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
            {
                CamConfig.CamState = ECameraState.Error;
                Thread.Sleep(100);
                throw new CVSCameraException(status, ECameraError.CamOpenFail);
            }

            CamConfig.HDevice = hDevice;

            SetCamCallback();

            if (!isReConnect)
            {
                // 재 연결일 경우 아래 동작 할 필요 없음 IntPtr을 재 할당하기 때문에 과거 이미지들 문제 생길 수 있음.
                UpdateCameraDatas();
            }

            //TODO : TriggerMode Test
            SetParameter(ECameraGetSetType.VFGEnum, VirtualFG40Library.MCAM_USER_SET_SELECTOR, VirtualFG40Library.USER_SET_SELECTOR_DEFAULT);
            SetParameter(ECameraGetSetType.VFGCmd, VirtualFG40Library.MCAM_USER_SET_LOAD, null);
            SetParameter(ECameraGetSetType.VFGInt, VirtualFG40Library.GEV_SCPS_PACKETSIZE, 8192);
            SetParameter(ECameraGetSetType.VFGEnum, VirtualFG40Library.MCAM_TRIGGER_MODE, VirtualFG40Library.TRIGGER_MODE_ON);
            SetParameter(ECameraGetSetType.VFGEnum, VirtualFG40Library.MCAM_TRIGGER_SOURCE, VirtualFG40Library.TRIGGER_SOURCE_SOFTWARE);
            SetParameter(ECameraGetSetType.VFGEnum, VirtualFG40Library.MCAM_PIXEL_FORMAT, VirtualFG40Library.PIXEL_FORMAT_MONO8);

            CamConfig.CamState = ECameraState.Opened;
            CamConfig.IsOpen = true;
        }

        public void Close()
        {
            _logWrite?.Info($"({CamConfig.UserID}) Crevis GigE Camera. Close.");

            try { AcqStop(); } catch { }

            CamConfig.IsOpen = false;

            GCHandleFree();

            _vfg?.CloseDevice(CamConfig.HDevice);
        }

        // TODO : Test
        public ulong GetPacketLossCount()
        {
            ulong total = 0;
            ulong packetLossCount = 0;

            int err = _vfg.GetTotalPacketCount(CamConfig.HDevice, ref total, ref packetLossCount);

            return packetLossCount;
        }

        /// <summary>
        /// 카메라 정보들(Width, Height, PixelFormat. BufferSize, Stride) 업데이트
        /// </summary>
        public void UpdateCameraDatas()
        {
            _logWrite?.Info($"({CamConfig.UserID}) Crevis GigE Camera. UpdateCameraDatas.");

            CamConfig.Width = (int)GetParameter(ECameraGetSetType.VFGInt, VirtualFG40Library.MCAM_WIDTH);
            CamConfig.Height = (int)GetParameter(ECameraGetSetType.VFGInt, VirtualFG40Library.MCAM_HEIGHT);
            CamConfig.VfgPixelFormat = (string)GetParameter(ECameraGetSetType.VFGEnum, VirtualFG40Library.MCAM_PIXEL_FORMAT);

            CamConfig.ExposeureTime = (double)GetParameter(ECameraGetSetType.VFGFloat, VirtualFG40Library.MCAM_EXPOSURE_TIME);
            CamConfig.Gain = (int)GetParameter(ECameraGetSetType.VFGInt, VirtualFG40Library.MCAM_GAIN_RAW);

            switch (CamConfig.VfgPixelFormat)
            {
                case "Mono8":
                    CamConfig.Buffersize = CamConfig.Width * CamConfig.Height;
                    CamConfig.Stride = (Int32)((CamConfig.Width * 8 + 7) / 8);
                    CamConfig.BitmapPixelFormat = PixelFormat.Format8bppIndexed;
                    CamConfig.IsNeedGrayScale = true;
                    break; ;
                case "BayerRG8":
                    CamConfig.Buffersize = CamConfig.Width * CamConfig.Height;
                    CamConfig.Stride = (Int32)((CamConfig.Width * 24 + 7) / 8);
                    CamConfig.BitmapPixelFormat = PixelFormat.Format24bppRgb;
                    CamConfig.IsNeedGrayScale = false;
                    break;

                default:
                        CamConfig.CamState = ECameraState.Error;
                        throw new CVSCameraException(null, ECameraError.UnimplementedPixelFormat, CamConfig.VfgPixelFormat);
            }
        }

        public void AcqStart()
        {
            if (CamConfig.IsOpen && !CamConfig.IsAcqStart)
            {
                _logWrite?.Info($"({CamConfig.UserID}) Crevis GigE Camera. AcqStart.");

                int status = _vfg.AcqStart(CamConfig.HDevice);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    CamConfig.CamState = ECameraState.Error;
                    throw new CVSCameraException(status, ECameraError.AcqStartFail);
                }
                //Thread.Sleep(2500); // 이거 안해주면 특정 카메라 모델에서 라이브러리단에서 튕김, 카메라마다 안전한 Sleep정도가 다름
                CamConfig.CamState = ECameraState.AcqStart;
                CamConfig.IsAcqStart = true;
            }
        }

        public void AcqStop()
        {
            if (CamConfig.IsOpen && CamConfig.IsAcqStart)
            {
                _logWrite?.Info($"({CamConfig.UserID}) Crevis GigE Camera. AcqStop.");

                int status = _vfg.AcqStop(CamConfig.HDevice);
                if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                {
                    CamConfig.CamState = ECameraState.Error;
                    throw new CVSCameraException(status, ECameraError.AcqStopFail);
                }
                //Thread.Sleep(2500); // 이거 안해주면 특정 카메라 모델에서 라이브러리단에서 튕김, 카메라마다 안전한 Sleep정도가 다름
                CamConfig.CamState = ECameraState.Opened;
                CamConfig.IsAcqStart = false;
            }
        }
        /// <summary>
        /// 이미지 그랩 시작 준비
        /// </summary>
        /// <exception cref="ThreadException"></exception>
        public void GrabStart()
        {
            _logWrite?.Info($"({CamConfig.UserID}) Crevis GigE Camera. GrabStart.");

            //if (_imgDataThread.IsAlive)
            //{
            //    _imgDataThread.Abort();
            //    // 5초안에 안죽으면 에러.
            //    if (!_imgDataThread.Join(5000))
            //    {
            //        CamConfig.CamState = ECameraState.Error;
            //        throw new ThreadException(EThreadError.DoesntDie, _imgDataThread.Name);
            //    }
            //}

            lock (_rawDatasLock)
            {
                for (int i = 0; i < RawDatas.Count; i++)
                {
                    RawDatas.TryDequeue(out _);
                }
            }
            
            //while (ReceiveImageDatas.TryDequeue(out Bitmap reData)) { reData.Dispose(); }

            CamConfig.CamState = ECameraState.GrabStart;
            CamConfig.IsGrabStart = true;
            _isDoingStop = false;

            //_imgDataThread = new Thread(new ThreadStart(ImageProcess)) { Name = $"CVSGigE_ImageProcessThread({CamConfig.UserID})" };
            //_imgDataThread.Start();
        }

        public void GrabStop()
        {
            _logWrite?.Info($"({CamConfig.UserID}) Crevis GigE Camera. GrabStop.");

            CamConfig.IsGrabStart = false;

            lock (_rawDatasLock)
            {
                for (int i = 0; i < RawDatas.Count; i++)
                {
                    RawDatas.TryDequeue(out _);
                }
            }
        }

        public void SWTriggerExecute(ICamera cam)
        {
            // Trigger Execute
            int errCode = _vfg.SetCmdReg(cam.CamConfig.HDevice, VirtualFG40Library.MCAM_TRIGGER_SOFTWARE);
            if (errCode != VirtualFG40Library.MCAM_ERR_SUCCESS)
            {
                CamConfig.CamState = ECameraState.Error;
                throw new Exception("Software trigger command failed");
            }
        }

        public void StopContinueTrigExecute()
        {
            _isSWTrigThread = false;

            if (_swTrigThread.IsAlive)
            {
                if (_swTrigThread.Join(500))
                    _swTrigThread.Abort();
            }
        }

        public void ContinueSWTrigExecute()
        {
            _isSWTrigThread = true;

            _swTrigThread = new Thread(new ThreadStart(TrigExecuteProcess));
            _swTrigThread.Name = "LiveCam Thread";
            _swTrigThread.Start();
        }

        private void TrigExecuteProcess()
        {
            int errCount = 0;
            try
            {
                while (_isSWTrigThread)
                {
                    try
                    {
                        int errCode = _vfg.SetCmdReg(CamConfig.HDevice, VirtualFG40Library.MCAM_TRIGGER_SOFTWARE);
                        if (errCode != VirtualFG40Library.MCAM_ERR_SUCCESS)
                            errCount++;

                        Thread.Sleep(400);
                    }
                    catch (Exception err)
                    {
                        _logWrite.Error(err, false, true);

                        if (errCount > 100)
                        {
                            CamConfig.CamState = ECameraState.Error;
                            throw new Exception("LiveCam thread die");
                        }

                        errCount++;
                    }
                }
            }
            catch (Exception err)
            {
                CamConfig.CamState = ECameraState.Error;
                _logWrite.Error(err);
            }
        }

        private void SetCamCallback()
        {
            _logWrite?.Info($"({CamConfig.UserID}) Crevis GigE Camera. Set Cam Callback.");

            int status = VirtualFG40Library.MCAM_ERR_SUCCESS;

            // 그랩시 동작하는 콜백 이벤트 등록
            VirtualFG40Library.CallbackFunc grab_callback_func = new VirtualFG40Library.CallbackFunc(OnNewImage);
            status = _vfg.SetCallbackFunction(CamConfig.HDevice, VirtualFG40Library.EVENT_NEW_IMAGE, grab_callback_func, new IntPtr());
            if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
            {
                CamConfig.CamState = ECameraState.Error;
                throw new CVSCameraException(status, ECameraError.SetCamCallbackFail);
            }

            // 카메라 연결 끊겼을 때 동작하는 콜백 이벤트 등록
            VirtualFG40Library.CallbackEvent event_callback_func = new VirtualFG40Library.CallbackEvent(OnDisconnected);
            status = _vfg.SetEventCallbackFunction(CamConfig.HDevice, VirtualFG40Library.EVENT_DEVICE_DISCONNECT, event_callback_func, new IntPtr());
            if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
            {
                CamConfig.CamState = ECameraState.Error;
                throw new CVSCameraException(status, ECameraError.SetCamEventCallbackFail);
            }

            // 가비지 컬렉션이 회수하지 못하도록.
            _gcHandle = GCHandle.Alloc(grab_callback_func);
            _gcHandleEvent = GCHandle.Alloc(event_callback_func);
        }

        /// <summary>
        /// Parse를 하지 않고 (double)로 진행하는 이유 => Parse/TryParse 함수를 하면 특정 자리수를 넘어가면 올림을 해버림 ;;
        /// ex) - 27227761.666666668 —Parse—> 27227761.6666667
        /// </summary>
        public void SetParameter(ECameraGetSetType camGetSetType, object command, object value)
        {
            if (command.ToString() != VirtualFG40Library.MCAM_TRIGGER_SOFTWARE)
                _logWrite?.Info($"({CamConfig.UserID}) Crevis GigE Camera. Set Parameter. Command = {command}, Value = {value}");

            var status = 0;
            switch (camGetSetType)
            {
                case ECameraGetSetType.VFGEnum:
                    status = _vfg.SetEnumReg(CamConfig.HDevice, command.ToString(), (string)value);
                    if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                    {
                        CamConfig.CamState = ECameraState.Error;
                        throw new CVSCameraException(status, ECameraError.SetEnumFail);
                    }
                    break;
                case ECameraGetSetType.VFGString:
                    status = _vfg.SetStrReg(CamConfig.HDevice, command.ToString(), (string)value);
                    if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                    {
                        CamConfig.CamState = ECameraState.Error;
                        throw new CVSCameraException(status, ECameraError.SetStrFail);
                    }
                    break;
                case ECameraGetSetType.VFGInt:
                    status = _vfg.SetIntReg(CamConfig.HDevice, command.ToString(), (int)value);
                    if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                    {
                        CamConfig.CamState = ECameraState.Error;
                        throw new CVSCameraException(status, ECameraError.SetIntFail);
                    }
                    break;
                case ECameraGetSetType.VFGFloat:
                    status = _vfg.SetFloatReg(CamConfig.HDevice, command.ToString(), (double)value);
                    if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                    {
                        CamConfig.CamState = ECameraState.Error;
                        throw new CVSCameraException(status, ECameraError.SetFloatFail);
                    }
                    break;
                case ECameraGetSetType.VFGBool:
                    status = _vfg.SetBoolReg(CamConfig.HDevice, command.ToString(), (bool)value);
                    if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                    {
                        CamConfig.CamState = ECameraState.Error;
                        throw new CVSCameraException(status, ECameraError.SetBoolFail);
                    }
                    break;
                case ECameraGetSetType.VFGCmd:
                    status = _vfg.SetCmdReg(CamConfig.HDevice, command.ToString());
                    if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                    {
                        CamConfig.CamState = ECameraState.Error;
                        throw new CVSCameraException(status, ECameraError.SetCmdFail);
                    }
                    break;
            }
        }

        public object GetParameter(ECameraGetSetType camGetSetType, object command)
        {
            _logWrite?.Info($"({CamConfig.UserID}) Crevis GigE Camera. Get Parameter. Command = {command}");

            var status = 0;
            object reVal = null;
            byte[] arrValue; int iValue; double dValue; bool bValue; uint size = 256;
            switch (camGetSetType)
            {
                case ECameraGetSetType.VFGEnum:
                    status = _vfg.GetEnumReg(CamConfig.HDevice, command.ToString(), null, ref size);
                    if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                    {
                        CamConfig.CamState = ECameraState.Error;
                        throw new CVSCameraException(status, ECameraError.GetEnumFail);
                    }
                    arrValue = new byte[size];
                    _vfg.GetEnumReg(CamConfig.HDevice, command.ToString(), arrValue, ref size);

                    reVal = Encoding.Default.GetString(arrValue);
                    break;
                case ECameraGetSetType.VFGString:
                    status = _vfg.GetStrReg(CamConfig.HDevice, command.ToString(), null, ref size);
                    if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                    {
                        CamConfig.CamState = ECameraState.Error;
                        throw new CVSCameraException(status, ECameraError.GetStrFail);
                    }
                    arrValue = new byte[size];
                    _vfg.GetStrReg(CamConfig.HDevice, command.ToString(), arrValue, ref size);

                    reVal = Encoding.Default.GetString(arrValue);
                    break;
                case ECameraGetSetType.VFGInt:
                    iValue = 0;
                    status = _vfg.GetIntReg(CamConfig.HDevice, command.ToString(), ref iValue);
                    if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                    {
                        CamConfig.CamState = ECameraState.Error;
                        throw new CVSCameraException(status, ECameraError.GetIntFail);
                    }

                    reVal = iValue;
                    break;
                case ECameraGetSetType.VFGFloat:
                    dValue = 0;
                    status = _vfg.GetFloatReg(CamConfig.HDevice, command.ToString(), ref dValue);
                    if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                    {
                        CamConfig.CamState = ECameraState.Error;
                        throw new CVSCameraException(status, ECameraError.GetFloatFail);
                    }

                    reVal = dValue;
                    break;
                case ECameraGetSetType.VFGBool:
                    bValue = false;
                    status = _vfg.GetBoolReg(CamConfig.HDevice, command.ToString(), ref bValue);
                    if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                    {
                        CamConfig.CamState = ECameraState.Error;
                        throw new CVSCameraException(status, ECameraError.GetBoolFail);
                    }

                    reVal = bValue;
                    break;
            }

            _logWrite?.Info($"({CamConfig.UserID}) Crevis GigE Camera. Get Parameter Compete. Value = {reVal}");

            return reVal;
        }

        /// <summary>
        /// 이미지 들어옴
        /// </summary>
        private int OnNewImage(int nEventID, IntPtr pBuffer, IntPtr pUserData)
        {
            try
            {
                // 다른 이벤트라면 종료
                if (nEventID != VirtualFG40Library.EVENT_NEW_IMAGE)
                    return 0;

                // Grab중이 아니라면 종료
                if (!CamConfig.IsGrabStart)
                    return 0;

                // Queue에 너무 많이 쌓였다면 에러 발생
                if (RawDatas.Count > MaxEnqueueCount)
                {
                    lock (_stoppingLock)
                    {
                        if (_isDoingStop) // 이미 종료중이라면 return
                            return 0;

                        _isDoingStop = true;
                    }

                    AcqStop();
                    GrabStop();
                    CamConfig.CamState = ECameraState.Error;
                    throw new CVSCameraException(null, ECameraError.CallbackQueueIsFull, $"UserID = {CamConfig.UserID}");
                }

                ProcessingCallback(pBuffer);

            }
            catch (Exception ex) 
            {
                CamConfig.CamState = ECameraState.Error;
                _logWrite?.Error(ex); 
            }

            return 0;
        }

        private void ProcessingCallback(IntPtr pBuffer)
        {
            byte[] arrImg = new byte[CamConfig.Buffersize];
            Marshal.Copy(pBuffer, arrImg, 0, arrImg.Length);

            lock (_rawDatasLock)
            {
                RawDatas.Enqueue(arrImg);
            }

            ReceiveRawDataEnqueueComplete?.Invoke(this, CamConfig.UserID);
            //_imgCallbackOnEvent.Set();
        }

        /// <summary>
        /// 카메라 연결 끊김
        /// </summary>
        private int OnDisconnected(int nEventID, IntPtr pUserData)
        {
            // TODO : 카메라 연결 끊길 시 에러발생시킬지 Info발생시킬지. 현재는 Info
            _logWrite?.Info($"({CamConfig.UserID}) Crevis GigE Camera. Disconnected!!");

            CamConfig.IsOpen = false;

            try
            {
                if (_reconnectThread.IsAlive)
                {
                    _reconnectThread.Abort();
                    _reconnectThread.Join(1000);
                }

                _reconnectThread = new Thread(new ThreadStart(TryReConnect)) { Name = $"ReConnectThread({CamConfig.UserID})" };
                _reconnectThread.Start();
            }
            catch (Exception ex) 
            {
                CamConfig.CamState = ECameraState.Error;
                _logWrite?.Error(ex); 
            }

            return 0;
        }

        private void TryReConnect()
        {
            try
            {
                _logWrite?.Info($"({CamConfig.UserID}) Crevis GigE Camera. Close Device.");
                _vfg.CloseDevice(CamConfig.HDevice); // 카메라 모델? 펌웨어 버전? 에 따라서 여기서도 터지고
                _logWrite?.Info($"({CamConfig.UserID}) Crevis GigE Camear. Close Device Complete.");

                GCHandleFree();

                while (!CamConfig.IsOpen)
                {
                    CamConfig.CamState = ECameraState.Reconnecting;

                    if (_isDisposing)
                        return;

                    try
                    {
                        Open(true); // 여기서도 터지고
                    }
                    catch (ThreadAbortException) { throw; }
                    catch (CVSCameraException) { continue; }
                    catch (Exception ex) { _logWrite?.Error(ex); continue; }

                    Thread.Sleep(100);
                }
                
                _logWrite?.Info($"({CamConfig.UserID}) Crevis GigE Camera. Reconnect Complete.");

                if (CamConfig.IsAcqStart)
                {
                    CamConfig.IsAcqStart = false;
                    AcqStart(); // 여기서도 터짐 ㅋ
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception ex) 
            {
                CamConfig.CamState = ECameraState.Error;
                _logWrite?.Error(ex); 
            }
        }
        //public void ImageProcess()
        //{
        //    try
        //    {
        //        while (CamConfig.IsGrabStart)
        //        {
        //            try
        //            {
        //                if (RawDatas.Count > 0)
        //                {
        //                    // Queue에 너무 많이 쌓였다면 에러 발생.
        //                    if (ReceiveImageDatas.Count > MaxEnqueueCount)
        //                    {
        //                        AcqStop();
        //                        GrabStop();
        //                        CamConfig.CamState = ECameraState.Error;
        //                        throw new CVSCameraException(null, ECameraError.CallbackQueueIsFull, $"({CamConfig.UserID})");
        //                    }

        //                    DateTime dt = DateTime.Now;

        //                    byte[] rawImg;
        //                    lock (_rawDatasLock)
        //                    {
        //                        RawDatas.TryDequeue(out rawImg);
        //                    }

        //                    //IntPtr ptr = IntPtr.Zero;
        //                    //Marshal.Copy(rawImg, 0, ptr, rawImg.Length);
        //                    BitmapData bmpData = new BitmapData();
        //                    bmpData.Stride = CamConfig.Stride;
        //                    bmpData.Height = CamConfig.Height;
        //                    bmpData.Width = CamConfig.Width;
        //                    bmpData.PixelFormat = CamConfig.BitmapPixelFormat;

        //                    Bitmap bmp;
        //                    switch (CamConfig.VfgPixelFormat)
        //                    {
        //                        case "Mono8":
        //                            bmp = _bmpConverter.ByteArrayToBitmap(bmpData, rawImg, CamConfig.BitmapPixelFormat);
        //                            break;

        //                        //case "BayerRG8":
        //                        //    IntPtr cvtPtr = IntPtr.Zero;
        //                        //    cvtPtr = Marshal.AllocHGlobal(CamConfig.Buffersize * 3);
        //                        //    _vfg.CvtColor(ptr, cvtPtr, CamConfig.Width, CamConfig.Height, VirtualFG40Library.CV_BayerRG2RGB);
        //                        //    bmp = _bmpConverter.IntPtrToBitmap(CamConfig, ptr);
        //                        //    Marshal.FreeHGlobal(ptr);
        //                        //    break;
        //                        default:
        //                            throw new CVSCameraException(null, ECameraError.UnimplementedPixelFormat, $"({CamConfig.UserID}), (PixelFormat = {CamConfig.VfgPixelFormat})");
        //                    }

        //                    ReceiveImageDatas.Enqueue(bmp);

        //                    // 누군가 구독했다면 완료 이벤트 발생.
        //                    ReceiveImageDataEnqueueComplete?.Invoke(this, CamConfig.UserID);
        //                }
        //                else
        //                {
        //                    _imgCallbackOnEvent.WaitOne(2000);
        //                    //TODO : 여기서 리트라이 기능 구현하면 될 듯
        //                }
        //            }
        //            catch (ThreadAbortException) { throw; }
        //            catch (CVSCameraException ex) { _logWrite?.Error(ex, false, true); }
        //            catch (Exception ex) { _logWrite?.Error(ex, true, true); }
        //        }
        //    }
        //    catch (ThreadAbortException) 
        //    {
        //        CamConfig.CamState = ECameraState.Error;
        //    }
        //    finally
        //    {
        //        CamConfig.IsGrabStart = false;
        //    }
        //}

        private void GCHandleFree()
        {
            _logWrite?.Info($"({CamConfig.UserID}) Crevis GigE Camera. GCHandleFree.");

            if (_gcHandle.IsAllocated)
                _gcHandle.Free();
            if (_gcHandleEvent.IsAllocated)
                _gcHandleEvent.Free();
        }

        public void Dispose()
        {
            if (_isDisposing)
                return;

            _isDisposing = true;
            try { Close(); } catch { }

            try
            {
                if (_reconnectThread.IsAlive && !_reconnectThread.Join(100))
                    _reconnectThread.Abort();
            }
            catch { }

            //try
            //{
            //    CamConfig.IsGrabStart = false;
            //    if (_reconnectThread.IsAlive && !_imgDataThread.Join(100))
            //        _reconnectThread.Abort();
            //}
            //catch { }
        }
    }
}
