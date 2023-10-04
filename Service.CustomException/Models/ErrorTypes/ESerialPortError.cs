using System.ComponentModel;

namespace Service.CustomException.Models.ErrorTypes
{
    public enum ESerialPortError
    {
        [Description("Channel Value Length Error!")]
        ChannelValueLength,
        [Description("Write Timeout!")]
        WriteTimeout,
        [Description("Read Timeout!")]
        ReadTimeout,
    }
}
