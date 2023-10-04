using System;

namespace Service.CustomException.Services.ErrorService.HandledExceptions
{
    public class BootstrapperException : HandledException
    {
        private string _failName;
        private Exception _exception;

        public BootstrapperException(string failName, Exception ex)
        {
            _failName = failName;
            _exception = ex;
        }

        public override void GetTitleAndMessage(out string title, out string message)
        {
            title = $"Load {_failName} Fail!";
            message = _exception.ToString();
        } 
    }
}
