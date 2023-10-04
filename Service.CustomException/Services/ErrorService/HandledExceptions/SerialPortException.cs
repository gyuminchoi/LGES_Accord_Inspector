using Service.CustomException.Models.ErrorTypes;

namespace Service.CustomException.Services.ErrorService.HandledExceptions
{
    public class SerialPortException : HandledException
    {
        private string _userMessage = string.Empty;
        private ESerialPortError _spError;

        public SerialPortException(ESerialPortError spError, string userMessage = "")
        {
            _spError = spError;
            _userMessage = userMessage;
        }

        public override void GetTitleAndMessage(out string title, out string message)
        {
            title = "Serial Port Error!";
            message = GetMessage(_spError, _userMessage);
        }
    }
}
