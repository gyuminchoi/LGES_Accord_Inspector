using System.ComponentModel;

namespace Service.CustomException.Models.ErrorTypes
{
    public enum EParseError
    {
        [Description("Parse int Fail!")]
        ParseIntFail,
        [Description("Parse string Fail!")]
        ParseStringFail,
        [Description("Parse double Fail!")]
        ParseDoubleFail,
        [Description("Parse object[] Fail!")]
        ParseObjectArrFail,
    }
}
