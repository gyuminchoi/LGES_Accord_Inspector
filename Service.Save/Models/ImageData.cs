using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Save.Models
{
    public class ImageData : IDisposable
    {
        public Bitmap Bmp { get; set; }
        public InspectionLocate Locate { get; set; }
        public int Index { get; set; }
        public DateTime InspectionTime { get; set; }

        public void Dispose()
        {
            Bmp.Dispose();
        }
    }
}
