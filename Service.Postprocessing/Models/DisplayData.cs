using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Service.Postprocessing.Models
{
    public class DisplayData
    {
        public IntPtr PImage { get; set; }
        public int BoxCount { get; set; }
        public int BarcodeCount { get; set; }
        public bool IsPass { get; set; }
    }
}
