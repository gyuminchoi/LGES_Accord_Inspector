using System.Drawing;
using System.Drawing.Imaging;
using ViDi2;

namespace Service.DeepLearning.Services
{
    public class VPDLDrawManager
    {

        public void DrawLinesFromIRedView(ref Bitmap bmp, IRedMarking redMarking, int ngPenSize)
        {
            foreach (var view in redMarking.Views)
            {
                if (view.Regions.Count > 0)
                {
                    // NG
                    foreach (IRegion region in view.Regions)
                    {
                        PointF[] pointFs = new PointF[region.Outer.Count];
                        int i = 0;
                        foreach (ViDi2.Point vidiPoint in region.Outer)
                        {
                            pointFs[i] = new PointF((float)(vidiPoint.X + view.Pose.OffsetX), (float)(vidiPoint.Y + view.Pose.OffsetY));
                            i++;
                        }
                        DrawingLinesFromPoinFs(ref bmp, Color.FromArgb(0x80, Color.Red), ngPenSize, pointFs);
                    }
                }
            }
            
        }

        public void DrawingLinesFromPoinFs(ref Bitmap bmp, Color color, int penWidth, PointF[] pointFs)
        {
            BitmapPixelFormatCheck(ref bmp);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                using (Pen pen = new Pen(color))
                {
                    pen.Width = penWidth;
                    g.DrawLines(pen, pointFs);
                }
            }
        }

        /// <summary>
        /// 8bppIndexed면 24bppRgb로 바꿔줌 (Graphic은 Indexed포멧에서 사용 불가.)
        /// </summary>
        public void BitmapPixelFormatCheck(ref Bitmap bmp)
        {
            if (bmp.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                Bitmap newBmp = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format24bppRgb);
                bmp.Dispose();
                bmp = newBmp;
            }
        }

        public void DrawingBorderFromRectangle(ref Bitmap bmp, Color color, int penWidth, Rectangle rectangle)
        {
            BitmapPixelFormatCheck(ref bmp);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                using (Pen pen = new Pen(color))
                {
                    pen.Width = penWidth;
                    g.DrawRectangle(pen, rectangle);
                }
            }
        }
        public void DrawingBorderFromIRedView(ref Bitmap bmp, Color color, IRedMarking redMarking, int penSize)
        {
            // NG
            foreach (var view in redMarking.Views)
            {
                int halfPen = penSize / 2;
                PointF[] pointFs = new PointF[6];
                pointFs[0] = new PointF((float)view.Pose.OffsetX + halfPen, (float)view.Pose.OffsetY + halfPen);
                pointFs[1] = new PointF((float)view.Pose.OffsetX - halfPen + (float)view.Size.Width, (float)view.Pose.OffsetY + halfPen);
                pointFs[2] = new PointF((float)view.Pose.OffsetX - halfPen + (float)view.Size.Width, (float)view.Pose.OffsetY + (float)view.Size.Height - halfPen);
                pointFs[3] = new PointF((float)view.Pose.OffsetX + halfPen, (float)view.Pose.OffsetY + (float)view.Size.Height - halfPen);
                pointFs[4] = new PointF((float)view.Pose.OffsetX + halfPen, (float)view.Pose.OffsetY + halfPen);
                pointFs[5] = new PointF((float)view.Pose.OffsetX - halfPen + (float)view.Size.Width, (float)view.Pose.OffsetY + halfPen);
                DrawingLinesFromPoinFs(ref bmp, color, penSize, pointFs);
            }
            
        }
    }
}
