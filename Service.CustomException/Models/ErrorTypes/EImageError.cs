using System.ComponentModel;

namespace Service.CustomException.Models.ErrorTypes
{
    public enum EImageError
    {
        [Description("Bitmap is Null!")]
        BitmapIsNull,

        [Description("Unsupported PixelFormat!")]
        UnsupportedPixelFormat,
    }
}
