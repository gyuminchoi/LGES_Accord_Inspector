using Crevis.VirtualFG40Library;
using Service.Camera.Models;
using Service.CustomException.Models.ErrorTypes;
using Service.CustomException.Services.ErrorService.HandledExceptions;
using Service.Logger.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Camera.Services
{
    public class CameraManager : ICameraManager
    {
        private VirtualFG40Library _vfg = new VirtualFG40Library();
        private LogWrite _logWrite = LogWrite.Instance;
        public Dictionary<string, ICamera> CameraDic { get; set; } = new Dictionary<string, ICamera>();

        public void Opens()
        {
            try
            {
                VirtualFGInit();
                UpdateDevice();

                uint camCount = CanOpenCameraNum();
                for (uint i = 0; i < camCount; i++)
                {
                    TryConnect(i, CameraDic);
                }

                _logWrite.Info("Successed Camera Init");
            }
            catch (Exception err)
            {
                _logWrite.Error(err);
            }
        }

        public void Closes()
        {
            foreach (var item in CameraDic.Values)
            {
                item.Close();
            }
        }

        public void AcqStarts()
        {
            foreach (var item in CameraDic.Values)
            {
                item.AcqStart();
            }
        }

        public void AcqStops()
        {
            foreach (var item in CameraDic.Values)
            {
                item.AcqStop();
            }
        }

        public void GrabStarts()
        {
            foreach (var item in CameraDic.Values)
            {
                item.GrabStart();
            }
        }

        public void GrabStops()
        {
            foreach (var item in CameraDic.Values)
            {
                item.GrabStop();
            }
        }

        public void SetParameters(ECameraGetSetType camGetSetType, string command, object value)
        {
            foreach (var item in CameraDic.Values)
            {
                item.SetParameter(camGetSetType, command, value);
            }
        }

        public List<bool> OpenChecks()
        {
            List<bool> flags = new List<bool>();
            foreach (var item in CameraDic)
            {
                flags.Add(item.Value.CamConfig.IsOpen);
            }
            return flags;
        }

        private void VirtualFGInit()
        {
            // VirtualFG Init
            var isInit = false;
            _vfg.IsInitSystem(ref isInit);

            int status = isInit ? VirtualFG40Library.MCAM_ERR_SUCCESS : _vfg.InitSystem();
            if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                throw new CVSCameraException(status, ECameraError.VFGInitFail);
        }

        private void UpdateDevice()
        {
            // 디바이스 목록 업데이트 // TODO : 이거 계속해도 뻑 안나는지 확인해볼것.
            int status = _vfg.UpdateDevice();
            if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                throw new CVSCameraException(status, ECameraError.UpdateDeviceFail);
        }

        private uint CanOpenCameraNum()
        {

            uint reVal = 0;
            // 연결 가능 카메라 수
            int status = _vfg.GetAvailableCameraNum(ref reVal);
            if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                throw new CVSCameraException(status, ECameraError.GetAvailableCameranumFail);

            _logWrite?.Info($"Crevis GigE Try Open Camera num - {reVal}");
            return reVal;
        }

        private bool TryConnect(uint camNum, Dictionary<string, ICamera> cameraDict)
        {
            _logWrite?.Info($"Crevis GigE Connect - TryConnect ({camNum})");

            bool isConnect = false;

            GetCVSCameraInfos(camNum, out string userID, out string modelName, out string serialNumber, out string deviceVersion);

            // 카메라 버그
            // 하나 연결한 다음 UpdateDevice한번 더 하면 이미 연결된 카메라는 UserID와 DeviceVersion이 ""로 떨어짐, S/N, ModelName은 정상. 
            if (deviceVersion == "" && userID == "")
                return false;

            if (!cameraDict.ContainsKey(userID))
            {
                CVSGigECamera cvsGigECam = new CVSGigECamera(_vfg, camNum, userID, modelName, serialNumber, deviceVersion);

                try
                {
                    // hDevice 생성 부분
                    cvsGigECam.Open();
                    // Dictionary Add
                    cameraDict.Add(cvsGigECam.CamConfig.UserID, cvsGigECam);
                    _logWrite?.Info($"Crevis GigE Camera Connect Success ({cvsGigECam.CamConfig.UserID})");
                }
                catch (Exception ex)
                {
                    _logWrite?.Info("Crevis GigE Camera Connect Fail" + Environment.NewLine + ex.ToString());
                    cvsGigECam.Dispose();
                }
            }

            return isConnect;
        }

        private void GetCVSCameraInfos(uint camIndex, out string userID, out string modelName, out string serialNumber, out string deviceVersion)
        {
            // Get User ID 
            userID = GetCVSEnumDeviceInfo(camIndex, VirtualFG40Library.MCAM_DEVICEINFO_USER_ID);

            // Get Model Name
            modelName = GetCVSEnumDeviceInfo(camIndex, VirtualFG40Library.MCAM_DEVICEINFO_MODEL_NAME);

            // Get Serial Number
            serialNumber = GetCVSEnumDeviceInfo(camIndex, VirtualFG40Library.MCAM_DEVICEINFO_SERIAL_NUMBER);

            // Get DeviceVersion
            deviceVersion = GetCVSEnumDeviceInfo(camIndex, VirtualFG40Library.MCAM_DEVICEINFO_DEVICE_VERSION);
        }

        private string GetCVSEnumDeviceInfo(uint hDevice, int code)
        {
            uint size = 256;
            var pInfo = new byte[size];

            int status = _vfg.GetEnumDeviceInfo(hDevice, code, pInfo, ref size);
            if (status != VirtualFG40Library.MCAM_ERR_SUCCESS)
                throw new CVSCameraException(status, ECameraError.GetEnumDeviceInfoFail);

            var temp = Encoding.ASCII.GetString(pInfo);

            return temp.Remove(temp.IndexOf('\0'));
        }
    }
}
