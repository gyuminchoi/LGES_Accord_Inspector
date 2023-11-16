namespace Service.Camera.Models
{
    public enum ECameraState
    {
        Opened,
        AcqStart,
        GrabStart,
        Error,
        Setting,
        Reconnecting
    }
}
