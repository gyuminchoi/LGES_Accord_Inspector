using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Camera.Models
{
    public class CameraConfig : BindableBase
    {
        #region Fields For Properties
        private ECameraType _cameraType;
        private uint _camIndex;
        private int _hDevice;
        private string _userID;
        private string _modelName;
        private string _serialNumber;
        private string _deviceVersion;
        private int _width;
        private int _height;
        private int _buffersize;
        private int _stride;
        private bool _isNeedGrayScale;
        private string _vfgPixelFormat;
        private string _euresysCLPixelFormat;
        private double _expTime;
        private int _gain;
        private PixelFormat _bitmapPixelFormat;
        private bool _isOpen = false;
        private bool _isAcqStart = false;
        private bool _isGrabStart = false;
        private ECameraState _camState;
        private string _vendor = "Crevis";
        private int _fps = 5;
        // For CL
        private string _portIDCL;
        private string _deviceIDCL;
        private uint _multicamChannel;
        #endregion

        // 카메라 종류
        public ECameraType CameraType { get => _cameraType; set => SetProperty(ref _cameraType, value); }

        // 카메라 최초 오픈 시 필요한 정보
        public uint CamIndex { get => _camIndex; set => SetProperty(ref _camIndex, value); }
        public string PortIDCL { get => _portIDCL; set => SetProperty(ref _portIDCL, value); }
        public string DeviceIDCL { get => _deviceIDCL; set => SetProperty(ref _deviceIDCL, value); }

        // 카메라 라이브러리 컨트롤을 위한 정보
        public int HDevice { get => _hDevice; set => SetProperty(ref _hDevice, value); }
        public uint MulticamChannel { get => _multicamChannel; set => SetProperty(ref _multicamChannel, value); }

        // 카메라 기본 정보
        public string UserID { get => _userID; set => SetProperty(ref _userID, value); }
        public string ModelName { get => _modelName; set => SetProperty(ref _modelName, value); }
        public string SerialNumber { get => _serialNumber; set => SetProperty(ref _serialNumber, value); }
        public string DeviceVersion { get => _deviceVersion; set => SetProperty(ref _deviceVersion, value); }
        public bool IsOpen { get => _isOpen; set => SetProperty(ref _isOpen, value); }
        public bool IsAcqStart { get => _isAcqStart; set => SetProperty(ref _isAcqStart, value); }
        public bool IsGrabStart { get => _isGrabStart; set => SetProperty(ref _isGrabStart, value); }

        // 카메라 밝기 컨트롤 정보
        public double ExposeureTime { get => _expTime; set => SetProperty(ref _expTime, value); }
        public int Gain { get => _gain; set => SetProperty(ref _gain, value); }

        // 이미지 프로세싱 위해 필요한 정보
        public int Width { get => _width; set => SetProperty(ref _width, value); }
        public int Height { get => _height; set => SetProperty(ref _height, value); }
        public int Buffersize { get => _buffersize; set => SetProperty(ref _buffersize, value); }
        public int Stride { get => _stride; set => SetProperty(ref _stride, value); }
        public bool IsNeedGrayScale { get => _isNeedGrayScale; set => SetProperty(ref _isNeedGrayScale, value); }
        public string VfgPixelFormat { get => _vfgPixelFormat; set => SetProperty(ref _vfgPixelFormat, value); }
        public string EuresysCLPixelFormat { get => _euresysCLPixelFormat; set => SetProperty(ref _euresysCLPixelFormat, value); }
        public PixelFormat BitmapPixelFormat { get => _bitmapPixelFormat; set => SetProperty(ref _bitmapPixelFormat, value); }

        // 카메라 상태
        public ECameraState CamState { get => _camState; set => SetProperty(ref _camState, value); }
        public string Vendor { get => _vendor; set => SetProperty(ref _vendor, value); }
        public int FPS { get => _fps; set => SetProperty(ref _fps, value); }
    }
}
