using System.ComponentModel;

namespace Service.CustomException.Models.ErrorTypes
{
    public enum EFnIOError
    {
        [Description("FnIO init fail!")]
        FnIOInitFail,
        [Description("Invaild IP address!")]
        InvalidIPAddress,
        [Description("FnIO open fail!")]
        FnIOOpenFail,
        [Description("FnIO set frequency fail!")]
        FnIOSetFrequencyFail,
        [Description("FnIO get slot number fail!")]
        FnIOGetSlotNumberFail,
        [Description("FnIO IO get IO module fail!")]
        FnIOIOGetIOModuleFail,
        [Description("FnIO get param fail!")]
        FnIOGetParamFail,
        [Description("FnIO set param fail!")]
        FnIOSetParamFail,
        [Description("FnIO update start fail!")]
        FnIOUpdateStartFail,
        [Description("FnIO write output bit fail!")]
        FnIOWriteOutputBitFail,
        [Description("FnIO write output byte fail!")]
        FnIOWriteOutputByteFail,
        [Description("FnIO read output bit fail!")]
        FnIOReadOutputBitFail,
        [Description("FnIO read Input bit fail!")]
        FnIOReadIntputBitFail,
        [Description("FnIO read Input byte fail!")]
        FnIOReadInputByteFail,
        [Description("FnIO signal check thread die!")]
        FnIOSignalCheckThreadDie,
        [Description("FnIO bus status check fail!")]
        FnIOBusCheckFail,
    }
}