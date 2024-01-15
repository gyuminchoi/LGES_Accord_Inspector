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
        public Bitmap OriginBmp { get; set; }
        public List<Box> BoxDatas { get; set; }
        public bool IsPass { get; set; }

        public VisionProResult(List<Box> boxDatas) 
        {
            BoxDatas = boxDatas;
        }
        public VisionProResult()
        {
                
        }

        public void Dispose()
        {
            OriginBmp.Dispose();
        }
    }
}
