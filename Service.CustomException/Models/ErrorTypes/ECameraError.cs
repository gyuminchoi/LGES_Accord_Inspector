using System.ComponentModel;

namespace Service.CustomException.Models.ErrorTypes
{
    // TODO : 에러 메시지 추가 부분. 만일 동일 에러 종류의 에러 메시지를 추가한다면 여기에서 내용만 추가하면 됨. 새로운 에러 종류 = enum클래스, Exception 하나 더파면 됨
    // 예시
    public enum ECameraError
    {
        [Description("VFG Init Fail!")]
        VFGInitFail,

        [Description("Update Device Fail!")]
        UpdateDeviceFail,

        [Description("Get Available Camera Num Fail!")]
        GetAvailableCameranumFail,

        [Description("Camera Open Fail!")]
        CamOpenFail,
        
        [Description("Set Enum Fail!")]
        SetEnumFail,

        [Description("Set String Fail!")]
        SetStrFail,

        [Description("Set Int Fail!")]
        SetIntFail,

        [Description("Set Float Fail!")]
        SetFloatFail,

        [Description("Set Bool Fail!")]
        SetBoolFail,

        [Description("Set Cmd Fail!")]
        SetCmdFail,

        [Description("Get Enum Device Info Fail!")]
        GetEnumDeviceInfoFail,

        [Description("Get Enum Fail!")]
        GetEnumFail,

        [Description("Get String Fail!")]
        GetStrFail,

        [Description("Get Int Fail!")]
        GetIntFail,

        [Description("Get Float Fail!")]
        GetFloatFail,

        [Description("Get Bool Fail!")]
        GetBoolFail,

        [Description("Acq Start Fail!")]
        AcqStartFail,

        [Description("Acq Stop Fail!")]
        AcqStopFail,

        [Description("Callback Queue is Full!")]
        CallbackQueueIsFull,

        [Description("Unimplemented PixelFormat!")]
        UnimplementedPixelFormat,

        [Description("Wrong CameraData!")]
        WrongCameraData,

        [Description("Can't Fount Camera!")]
        CannotFoundCamera,

        [Description("Camera Is Not Open!")]
        CameraIsNotOpen,

        [Description("Set Cam Callback Fail!")]
        SetCamCallbackFail,

        [Description("Set Cam Event Callback Fail!")]
        SetCamEventCallbackFail,

        [Description("Disconnected Exception!")]
        DisconnectedException,

        [Description("Try ReConnect Exception Inside While.")]
        TryReConnectInsideWhileException,

        [Description("Try ReConnect Exception!")]
        TryReconnectException,

        [Description("VFG-CL Init Fail!")]
        VFGCLInitFail,

        [Description("CL Get Device ID Fail!")]
        CLGetDeviceIDFail,

        [Description("CL Get Port ID Num Fail")]
        CLGetPortIDNumFail,

        [Description("CL Get Port ID Fail")]
        GetPortIDFail,

        [Description("CL Get Device ID Num Fail")]
        CLGetDeviceIDNumFail,

        [Description("CL Connect Device Fail")]
        CLConnectDeviceFail,

        [Description("CL Desconnect Device Fail")]
        CLDisconnectDeviceFail,
    }
}
