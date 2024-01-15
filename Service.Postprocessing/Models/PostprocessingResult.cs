using Service.VisionPro.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Postprocessing.Models
{
    public class PostprocessingResult : IDisposable
    {
        public Bitmap OverlayBmp { get; set; }
        public VisionProResult VisionProResult { get; set; }

        public PostprocessingResult()
        {

        }

        public void Dispose()
        {
            OverlayBmp.Dispose();
            VisionProResult.Dispose();
        }
    }
}
