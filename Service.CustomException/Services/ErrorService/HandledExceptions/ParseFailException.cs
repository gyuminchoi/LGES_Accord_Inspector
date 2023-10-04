using Service.CustomException.Models.ErrorTypes;

namespace Service.CustomException.Services.ErrorService.HandledExceptions
{
    public class ParseFailException : HandledException
    {
        private EParseError _parseError { get; set; }
        private string _userMessage { get; set; }
        public ParseFailException(EParseError parseError, string userMessage = "")
        {
            _parseError = parseError;
            _userMessage = userMessage;
        }
        public override void GetTitleAndMessage(out string title, out string message)
        {
            title = "Parse Fail!";

            message = GetMessage(_parseError, _userMessage);
        }
    }
}
