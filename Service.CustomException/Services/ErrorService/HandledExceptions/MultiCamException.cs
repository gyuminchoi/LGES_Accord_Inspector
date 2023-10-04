using Service.CustomException.Services.ErrorService.HandledExceptions;

namespace Service.CustomException.Service.ErrorService.HandledExceptions
{
    public class MultiCamException : HandledException
    {
        private string _muliCamMessage = string.Empty;

        public MultiCamException(string multiCamMessage)
        {
            _muliCamMessage = multiCamMessage;
        }

        public override void GetTitleAndMessage(out string title, out string message)
        {
            title = "MultiCam Error!";
            message = _muliCamMessage;
        }
    }
}
