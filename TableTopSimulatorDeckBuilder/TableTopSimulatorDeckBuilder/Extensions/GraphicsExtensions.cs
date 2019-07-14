using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace TableTopSimulatorDeckBuilder.Extensions
{
    public static class GraphicsExtensions
    {
        public static void WithGraphics(this Bitmap bitmap, Action<Graphics> action)
        {
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                action(graphics);
            }
        }
    }
}
