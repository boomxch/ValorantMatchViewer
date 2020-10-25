using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;

namespace ValorantMatchViewer
{
    public abstract class ImageProcessor
    {
        public abstract void ConvertImage(Bitmap bitmap);

        public Bitmap TrimImageFromFullHd(Bitmap bmp, Point fhdxy, Size fhdwh)
        {
            var rect = new Rectangle(
                ConvertFullHdToImgLength(fhdxy.X, true, bmp), ConvertFullHdToImgLength(fhdxy.Y, false, bmp),
                ConvertFullHdToImgLength(fhdwh.Width, true, bmp), ConvertFullHdToImgLength(fhdwh.Height, false, bmp));
            return bmp.Clone(rect, bmp.PixelFormat);
        }

        /// <summary>
        /// https://imagingsolution.net/program/csharp/rezise_bitmap_data/
        /// </summary>
        /// <param name="original"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="interpolationMode"></param>
        /// <returns></returns>
        public Bitmap ResizeBitmap(Bitmap original, int width, int height, InterpolationMode interpolationMode)
        {
            Bitmap bmpResize;
            Bitmap bmpResizeColor;
            Graphics graphics = null;

            try
            {
                var pf = original.PixelFormat;

                if (original.PixelFormat == PixelFormat.Format8bppIndexed)
                    pf = PixelFormat.Format24bppRgb;

                bmpResizeColor = new Bitmap(width, height, pf);
                var dstRect = new RectangleF(0, 0, width, height);
                var srcRect = new RectangleF(-0.5f, -0.5f, original.Width, original.Height);
                graphics = Graphics.FromImage(bmpResizeColor);
                graphics.Clear(Color.Transparent);
                graphics.InterpolationMode = interpolationMode;
                graphics.DrawImage(original, dstRect, srcRect, GraphicsUnit.Pixel);

            }
            finally
            {
                graphics?.Dispose();
            }

            if (original.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                bmpResize = new Bitmap(
                    bmpResizeColor.Width,
                    bmpResizeColor.Height,
                    PixelFormat.Format8bppIndexed
                    );

                var pal = bmpResize.Palette;
                for (var i = 0; i < bmpResize.Palette.Entries.Length; i++)
                    pal.Entries[i] = original.Palette.Entries[i];

                bmpResize.Palette = pal;

                var bmpDataColor = bmpResizeColor.LockBits(
                        new Rectangle(0, 0, bmpResizeColor.Width, bmpResizeColor.Height),
                        ImageLockMode.ReadWrite,
                        bmpResizeColor.PixelFormat
                        );

                var bmpDataMono = bmpResize.LockBits(
                        new Rectangle(0, 0, bmpResize.Width, bmpResize.Height),
                        ImageLockMode.ReadWrite,
                        bmpResize.PixelFormat
                        );

                var colorStride = bmpDataColor.Stride;
                var monoStride = bmpDataMono.Stride;

                unsafe
                {
                    var pColor = (byte*)bmpDataColor.Scan0;
                    var pMono = (byte*)bmpDataMono.Scan0;
                    for (var y = 0; y < bmpDataColor.Height; y++)
                    {
                        for (var x = 0; x < bmpDataColor.Width; x++)
                        {
                            if (pMono == null) continue;
                            if (pColor != null)
                                pMono[x + y * monoStride] = pColor[x * 3 + y * colorStride];
                        }
                    }
                }

                bmpResize.UnlockBits(bmpDataMono);
                bmpResizeColor.UnlockBits(bmpDataColor);

                bmpResizeColor.Dispose();
            }
            else
            {
                bmpResize = bmpResizeColor;
            }

            return bmpResize;
        }

        public int ConvertFullHdToImgLength(int length, bool isHorizontal, Image bmp)
        {
            var d = isHorizontal ? (double)length / 1920 : (double)length / 1080;
            return (int)Math.Round(d * (isHorizontal ? bmp.Width : bmp.Height));
        }

        public int CountWhiteArea(Bitmap bitmap)
        {
            const int threshold = 230;

            var ans = 0;

            var data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);
            var buf = new byte[bitmap.Width * bitmap.Height * 4];
            Marshal.Copy(data.Scan0, buf, 0, buf.Length);

            for (var i = 0; i < buf.Length; i += 4)
                if (buf[i] >= threshold && buf[i + 1] >= threshold && buf[i + 2] >= threshold) ans++;

            bitmap.UnlockBits(data);

            return ans;
        }
    }
}
