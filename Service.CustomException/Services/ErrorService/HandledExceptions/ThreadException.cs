using Service.CustomException.Models.ErrorTypes;
using Service.CustomException.Services.ConvertService;

namespace Service.CustomException.Services.ErrorService.HandledExceptions
{
    public class ThreadException : HandledException
    {
        private EThreadError _threadErrorType;
        private string _threadName;

        public ThreadException(EThreadError threadErrorType, string threadName)
        {
            _threadErrorType = threadErrorType;
            _threadName = threadName;
        }

        public override void GetTitleAndMessage(out string title, out string message)
        {
            title = "Thread Error!";
            message = EnumValueConverter.GetDescription(_threadErrorType) + " (Thread Name = " + _threadName + ")";
        }
    }
}
