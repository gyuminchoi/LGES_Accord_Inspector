using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViDi2;

namespace Service.DeepLearning.Models
{
    public class VPDLResult : IDisposable
    {
        public Bitmap OriginBmp { get; set; }
        public Bitmap OverlayBmp { get; set; }
        public bool IsPass { get; set; }

        public void Dispose()
        {
            OriginBmp?.Dispose();
            OverlayBmp?.Dispose();
        }
    }
}
