using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Service.ImageMerge.Models
{
    public class MergeBitmap : IDisposable
    {
        public Bitmap Bmp { get; set; }
        public IntPtr PImage { get; set; }

        public void Dispose()
        {
            if (PImage != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(PImage);
            }
            if (Bmp != null) Bmp.Dispose();
        }
    }
}
