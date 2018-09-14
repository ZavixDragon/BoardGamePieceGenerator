using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Generator
{
    public static class ImageExtensions
    {
        private const int _bytesPerPixel = 4;

        public static Image WithOpacity(this Image img, decimal opacity)
        {
            var bitmap = (Bitmap)img.Clone();
            PixelFormat pxf = PixelFormat.Format32bppArgb;
            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bmpData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, pxf);
            IntPtr ptr = bmpData.Scan0;
            int numBytes = bitmap.Width * bitmap.Height * _bytesPerPixel;
            byte[] argbValues = new byte[numBytes];
            Marshal.Copy(ptr, argbValues, 0, numBytes);
            for (int counter = 0; counter < argbValues.Length; counter += _bytesPerPixel)
            {
                // argbValues is in format BGRA (Blue, Green, Red, Alpha)
                // If 100% transparent, skip pixel
                if (argbValues[counter + _bytesPerPixel - 1] == 0)
                    continue;
                int pos = 0;
                pos++; // B value
                pos++; // G value
                pos++; // R value
                argbValues[counter + pos] = (byte)(argbValues[counter + pos] * opacity);
            }
            Marshal.Copy(argbValues, 0, ptr, numBytes);
            bitmap.UnlockBits(bmpData);
            return bitmap;
        }
    }
}
