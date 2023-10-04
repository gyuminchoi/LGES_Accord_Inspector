using Service.CustomException.Services.ConvertService;
using System;

namespace Service.CustomException.Services.ErrorService.HandledExceptions
{
    // Catch단에서 HandledException과 Exception을 나눠서 받기 위함.
    public abstract class HandledException : Exception 
    {
        public string Version { get; set; } = "2.0.0";

        public abstract void GetTitleAndMessage(out string title, out string message);

        protected string GetMessage(Enum en, string userMessage)
        {
            string message = $"{EnumValueConverter.GetDescription(en)}";

            if (!String.IsNullOrEmpty(userMessage))
                message += " - " + userMessage;

            return message;
        }
    }
}
