using System.Collections.Generic;
using System.Drawing;

namespace Service.Drawing.Services
{
    public class DrawingManager
    {
        public DrawingManager() { }

        //public void DrawRectangle(Bitmap bmp, Color color, int penWidth, RectangleF rectangle)
        //{
        //    using(Graphics g = Graphics.FromImage(bmp))
        //    {
        //        using(Pen pen = new Pen(color, penWidth))
        //        {
        //            g.DrawRectangle(pen, rectangle);
        //        }
        //    }
        //}

        
        public Bitmap DrawRectangles(Bitmap bmp, string color, int penSize, List<RectangleF> rectList)
        {
            RectangleF[] rectArr = rectList.ToArray();

            Color col = ColorTranslator.FromHtml(color);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                using (Pen pen = new Pen(col, penSize))
                {
                    g.DrawRectangles(pen, rectArr);
                }
                return bmp;
            }
        }
    }
}
