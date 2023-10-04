using Service.CustomException.Models.ErrorTypes;

namespace Service.CustomException.Services.ErrorService.HandledExceptions
{
    public class ImageException : HandledException
    {
        private EImageError _imgError;
        private string _userMessage = string.Empty;

        public ImageException(EImageError imgErr, string userMessgae = "")
        {
            _imgError = imgErr;
            _userMessage = userMessgae;
        }
        public override void GetTitleAndMessage(out string title, out string message)
        {
            title = "Image Error!";

            message = GetMessage(_imgError, _userMessage);
        }
    }
}
