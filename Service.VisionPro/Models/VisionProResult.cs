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
        public DateTime InspectionTime { get; set; }
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

        public int GetBarcodeCount()
        {
            int count = 0;
            
            foreach (Box box in BoxDatas)
            {
                if(box.Barcodes != null)
                    count += box.Barcodes.Count;
            }

            return count;
        }

        public void Dispose()
        {
            if (OriginBmp != null) { OriginBmp.Dispose(); }
            foreach (Box box in BoxDatas) {  box.Dispose(); }
        }
    }
}
