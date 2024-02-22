using Cognex.VisionPro.Exceptions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.VisionPro.Models
{
    public class VisionProResult : IDisposable
    {
        public Bitmap Bmp { get; set; }
        public bool IsPass { get; set; }

        public int X { get; set; }
        public int Y { get; set; }

        public VisionProResult()
        {
                
        }

        public void Dispose()
        {
            Bmp?.Dispose();
        }
    }
}
