using Service.CustomException.Models.ErrorTypes;
using System;

namespace Service.CustomException.Services.ErrorService.HandledExceptions
{
    // TODO : Exception(에러 타입) 추가 부분.
    // 필요한 Exception은 ex) CvsIOException, VidiException 등... 아래 예시처럼 클래스를 추가해주면 된다. + enum 클래스도 추가해주면 됨
    // 예시
    public class CVSCameraException : HandledException
    {
        private int? _vfgErrCode;
        private string _userMessage = string.Empty;
        private ECameraError _camError;

        public CVSCameraException(int? vfgCode, ECameraError camErr, string userMessage = "")
        {
            _vfgErrCode = vfgCode;
            _camError = camErr;
            _userMessage = userMessage;
        }


        public override void GetTitleAndMessage(out string title, out string message)
        {
            title = "CVS Camera Error!";

            message = GetMessage(_camError, _userMessage);

            if (_vfgErrCode != null)
                message += Environment.NewLine + $"(VFG Err Code : {_vfgErrCode})";
        }
    }
}
