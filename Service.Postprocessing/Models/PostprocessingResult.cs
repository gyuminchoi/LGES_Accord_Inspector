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

        public PostprocessingResult(VisionProResult vpResult)
        {
            VisionProResult = vpResult;
        }

        public void Dispose()
        {
            if(OverlayBmp != null) OverlayBmp.Dispose();
            if(VisionProResult != null) VisionProResult.Dispose();
        }
    }
}
