using Service.CustomException.Models.ErrorTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.CustomException.Services.ErrorService.HandledExceptions
{
    public class DisplacementSensorException : HandledException
    {
        private EDisplacementSensorError _sensorInterfaceError;
        private string _userMessage = string.Empty;

        public DisplacementSensorException(EDisplacementSensorError sensorInterfaceError, string userMessage = "")
        {
            _sensorInterfaceError = sensorInterfaceError;
            _userMessage = userMessage;
        }

        public override void GetTitleAndMessage(out string title, out string message)
        {
            title = "Sensor Interface Error!";

            message = GetMessage(_sensorInterfaceError, _userMessage);
        }
    }
}
