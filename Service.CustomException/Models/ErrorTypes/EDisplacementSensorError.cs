using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.CustomException.Models.ErrorTypes
{
    public enum EDisplacementSensorError
    {
        [Description("Incorrect Dialing Code!")]
        IncorrectDialingCode,
        [Description("Not Searched IO!")]
        NotSearchedIO,
        [Description("Not Received Transmit Acknowledge Signal!")]
        NotReceivedTASignal,
        [Description("Not Received Receive Request Signal!")]
        NotReceivedRRSignal,
        [Description("Received Error Command!")]
        ReaceivedErrorCommand,
        [Description("Not Changed Zero Set!")]
        NotChangedZeroSet,
        [Description("Invalid IO Index!")]
        InvalidIOIndex,
        [Description("Different values ​​come in!")]
        DifferentValue
    }
}
