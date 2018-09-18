using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Generator
{
    public static class ImageExtensions
    {
        private const int _bytesPerPixel = 4;
        private const int _alphaOffset = 3; //_bytesPerPixel - 1

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
                if (argbValues[counter + _alphaOffset] != 0)
                    argbValues[counter + _alphaOffset] = (byte)(argbValues[counter + _alphaOffset] * opacity);
            Marshal.Copy(argbValues, 0, ptr, numBytes);
            bitmap.UnlockBits(bmpData);
            return bitmap;
        }
    }
}
