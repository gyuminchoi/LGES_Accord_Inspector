using Service.CustomException.Models.ErrorTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.CustomException.Services.ErrorService.HandledExceptions
{
    public class VisionProException : HandledException
    {
        private EVisionProError _visionProError;
        private string _userMessage = string.Empty;

        public VisionProException(EVisionProError vpErr, string userMessage = "")
        {
            _visionProError = vpErr;
            _userMessage = userMessage;
        }

        public override void GetTitleAndMessage(out string title, out string message)
        {
            title = "VisionPro Error!";

            message = GetMessage(_visionProError, _userMessage);
        }
    }
}
