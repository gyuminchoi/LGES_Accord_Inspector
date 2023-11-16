using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Service.Camera.Models
{
    public class CamReceiveData : IDisposable
    {
        public Bitmap Bmp { get; set; }
        public DateTime GrabTime { get; set; }
        
        public CamReceiveData() { }
        ~CamReceiveData() => Dispose();

        public void Dispose()
        {
            if (Bmp != null) Bmp.Dispose();
        }
    }
}
