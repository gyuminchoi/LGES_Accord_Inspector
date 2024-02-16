using AForge.Imaging.Filters;
using Service.Camera.Models;
using Service.CustomException.Models.ErrorTypes;
using Service.CustomException.Services.ErrorService.HandledExceptions;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Service.Camera.Services.ConvertService
{
    public class BitmapConverter : LazySingleton<BitmapConverter>
    {
        public Bitmap IntPtrToBitmap(CameraConfig camConfig, IntPtr pImage)
        {
            Bitmap bmp;

            bmp = new Bitmap(camConfig.Width, camConfig.Height, camConfig.Stride, camConfig.BitmapPixelFormat, pImage);

            SetGrayScale(ref bmp);

            return bmp;
        }

        public Bitmap IntPtrToBitmap(BitmapData bmpData, IntPtr pImage)
        {
            Bitmap bmp = new Bitmap(bmpData.Width, bmpData.Height, bmpData.Stride, bmpData.PixelFormat, pImage);

            SetGrayScale(ref bmp);

            return bmp;
        }


        /// <summary>
        /// 얕은 복사 됨
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="pImage"></param>
        public void BitmapToInPtr(Bitmap bmp, ref IntPtr pImage)
        {
            BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
            bmp.UnlockBits(bmpdata);
            pImage = bmpdata.Scan0;
        }

        /// <summary>
        /// 깊은 복사
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        /// <param name="count"></param>
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);


        public Bitmap ByteArrayToBitmap(BitmapData originalData, byte[] source, PixelFormat format)
        {
            Bitmap bmp = new Bitmap(originalData.Width, originalData.Height, originalData.Width, format, new IntPtr());

            if (format == PixelFormat.Format8bppIndexed)
            {
                ColorPalette pal = bmp.Palette;
                for (int i = 0; i < 256; i++)
                {
                    pal.Entries[i] = Color.FromArgb(255, i, i, i);
                }
                bmp.Palette = pal;
            }

            BitmapData resultData = bmp.LockBits(new Rectangle(0, 0, originalData.Width, originalData.Height), ImageLockMode.ReadOnly, format);
            Marshal.Copy(source, 0, resultData.Scan0, resultData.Stride * resultData.Height);
            bmp.UnlockBits(resultData);

            return bmp;
        }

        public Bitmap ByteArrToBitmap(byte[] data, int width, int height, PixelFormat pixelFormat)
        {
            Bitmap bmp = new Bitmap(width, height, pixelFormat);

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

            Marshal.Copy(data, 0, bmpData.Scan0, data.Length);

            bmp.UnlockBits(bmpData);

            return bmp;
        }

        public BitmapImage BitmapToBitmapImage(Bitmap bmp)
        {
            var bi = new BitmapImage();
            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Bmp);
                ms.Position = 0;
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.StreamSource = ms;
                bi.EndInit();
            }
            bi.Freeze();
            return bi;
        }

        public Bitmap BitmapImageToBitmap(BitmapImage bmpimg)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bmpimg));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        public Bitmap ConvertTo24bpp(Bitmap originalBitmap)
        {
            try
            {// 24비트 RGB 포맷의 이미지로 변환
                Bitmap newBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                using (Graphics graphics = Graphics.FromImage(newBitmap))
                {
                    graphics.DrawImage(originalBitmap, 0, 0, originalBitmap.Width, originalBitmap.Height);
                }
                return newBitmap;
            }
            catch (Exception)
            {
                return originalBitmap;
            }
        }

        public Bitmap Get8GrayBitmap(Bitmap originBmp)
        {
            return Grayscale.CommonAlgorithms.BT709.Apply(originBmp);
        }

        public Bitmap BitmapImageTo8BitBitmap(BitmapImage bmpImage)
        {
            // BitmapImage -> 32bit Bitmap
            Bitmap bpp32Bmp = BitmapImageToBitmap(bmpImage);
            // 32bit -> 24bit
            Bitmap bpp24Bmp = ConvertTo24bpp(bpp32Bmp);
            // 24bit -> 8bit
            return Get8GrayBitmap(bpp24Bmp);
        }

        public void SetGrayScale(ref Bitmap bmp)
        {
            if (bmp == null) throw new ImageException(EImageError.BitmapIsNull);

            if (bmp.PixelFormat == PixelFormat.Format24bppRgb)
            {
                Bitmap newbmp = Grayscale.CommonAlgorithms.BT709.Apply(bmp);
                bmp.Dispose();
                bmp = newbmp;
            }
            else if (bmp.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                ColorPalette cp = bmp.Palette;
                for (int i = 0; i < 256; i++)
                    cp.Entries[i] = Color.FromArgb(i, i, i);
                bmp.Palette = cp;
            }
            else
                throw new ImageException(EImageError.UnsupportedPixelFormat, $"({bmp.PixelFormat})");
        }
    }
}
