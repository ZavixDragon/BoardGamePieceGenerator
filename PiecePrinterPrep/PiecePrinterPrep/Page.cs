using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using PiecePrinterPrep.Extensions;

namespace PiecePrinterPrep
{
    public class Page
    {
        public const int WIDTH = 2550;
        public const int HEIGHT = 3300;
        private readonly List<PositionedImage> _images = new List<PositionedImage>();
        private readonly int _pageNumber;
        private int _xOffset = 75;
        private int _yOffset = 75;

        public Page(int pageNumber)
        {
            _pageNumber = pageNumber;
        }

        public bool CanAdd(ImageToPrint image) 
            => _xOffset + image.Width < WIDTH 
            || _yOffset + image.Height * 2 < HEIGHT;

        public void Add(ImageToPrint image)
        {
            if (_xOffset + image.Width > WIDTH)
            {
                _xOffset = 75;
                _yOffset += image.Height;
            }
            _images.Add(new PositionedImage(_xOffset, _yOffset, image));
            _xOffset += image.Width;
        }

        public void Create(string directory, string saveName)
        {
            var frontBitmap = new Bitmap(WIDTH, HEIGHT);
            var backBitmap = new Bitmap(WIDTH, HEIGHT);
            using (var frontGraphics = Graphics.FromImage(frontBitmap))
            {
                frontGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                frontGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                frontGraphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                frontGraphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                using (var brush = new SolidBrush(Color.White))
                    frontGraphics.FillRectangle(brush, 0, 0, WIDTH, HEIGHT);
                using (var backGraphics = Graphics.FromImage(backBitmap))
                {
                    backGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                    backGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    backGraphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    backGraphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    using (var brush = new SolidBrush(Color.White))
                        backGraphics.FillRectangle(brush, 0, 0, WIDTH, HEIGHT);

                    _images.ForEach(x => x.Image.Apply(frontGraphics, backGraphics, x.X, x.Y, x.BackX, x.BackY));

                    frontGraphics.Flush();
                    backGraphics.Flush();
                    frontBitmap.Save(PathX.Build(directory, saveName + "Front" + _pageNumber + ".png"), ImageFormat.Png);
                    backBitmap.Save(PathX.Build(directory, saveName + "Back" + _pageNumber + ".png"), ImageFormat.Png);
                }
            }
            frontBitmap.Dispose();
            backBitmap.Dispose();
        }
    }

    public class PositionedImage
    {
        public int X { get; }
        public int BackX { get; }
        public int Y { get; }
        public int BackY { get; }
        public ImageToPrint Image { get; }

        public PositionedImage(int x, int y, ImageToPrint image)
        {
            X = x;
            BackX = Page.WIDTH - x - image.Width;
            Y = y;
            BackY = Y;
            Image = image;
        }
    }
}
