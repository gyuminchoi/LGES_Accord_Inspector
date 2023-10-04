using System.ComponentModel;

namespace Service.CustomException.Models.ErrorTypes
{
    public enum EThreadError
    {
        [Description("Thread doesn't die")]
        DoesntDie,
    }
}
