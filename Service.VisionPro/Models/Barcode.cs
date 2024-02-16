using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.VisionPro.Models
{
    public class Barcode
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string Code { get; set; }

        public Barcode() { }
        public Barcode(double x, double y, string code)
        {
            X = x;
            Y = y;
            Code = code;
        }
    }
}
