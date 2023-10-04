using Service.CustomException.Models.ErrorTypes;
using System;

namespace Service.CustomException.Services.ErrorService.HandledExceptions
{
    public class FnIOException : HandledException
    {
        private int? _fnIOLibErrCode;
        private EFnIOError _fnIOError;
        private string _userMessage = string.Empty;

        public FnIOException(int? fnIOLibErrCode, EFnIOError fnIOError, string userMessage = "")
        {
            _fnIOLibErrCode = fnIOLibErrCode;
            _fnIOError = fnIOError;
            _userMessage = userMessage;
        }

        public override void GetTitleAndMessage(out string title, out string message)
        {
            title = "FnIO Error!";

            message = GetMessage(_fnIOError, _userMessage);

            if (_fnIOLibErrCode != null)
                message += Environment.NewLine + $"(FnIO Lib Err Code : {_fnIOLibErrCode})";
        }
    }
}
