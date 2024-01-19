using Cognex.VisionPro;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.VisionPro.Models
{
    public class Box : IDisposable
    {
        public Bitmap CropBmp { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public List<Barcode> Barcodes { get; set; }
        public Box(Bitmap cropBmp, double x, double y, double width, double height)
        {
            CropBmp = cropBmp;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public void Dispose()
        {
            if(CropBmp != null) { CropBmp.Dispose(); }
        }
    }
}
