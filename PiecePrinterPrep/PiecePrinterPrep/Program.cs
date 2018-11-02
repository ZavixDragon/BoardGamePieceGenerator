using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.IO;
using System.Linq;

namespace PiecePrinterPrep
{
    class Program
    {
        static void Main(string[] args)
        {
            var lines = File.ReadAllLines(args[0]);
            var basePath = lines[0];
            var savePath = Path.Combine(lines[0], lines[1]);
            var pages = new Pages(savePath, lines[2]);
            lines.Skip(4).ToList().ForEach(item =>
            {
                var count = int.Parse(item.Split(' ').First());
                var path = item.Substring(item.Split(' ').First().Length + 1);
                var detail = new ImageDetail(basePath, path, int.Parse(lines[3]));
                Enumerable.Range(0, count).ToList().ForEach(x => pages.Add(detail));
            });
            pages.Create();
            pages.Dispose();
        }
    }

    public class Pages
    {
        private readonly List<Page> _pages = new List<Page>();
        private readonly string _saveDir;
        private readonly string _saveName;

        public Pages(string saveDir, string saveName)
        {
            _saveDir = saveDir;
            _saveName = saveName;
            _pages.Add(new Page(Path.GetFullPath(Path.Combine(_saveDir, $"{_saveName}{_pages.Count + 1}.png")), 2300, 3200));
        }

        public void Add(ImageDetail detail)
        {
            if (!_pages.Last().CanAdd(detail))
                _pages.Add(new Page(Path.GetFullPath(Path.Combine(_saveDir, $"{_saveName}{_pages.Count + 1}.png")), 2300, 3200));
            _pages.Last().Add(detail);
        }

        public void Create() => _pages.ForEach(x => x.Create());
        public void Dispose() => _pages.ForEach(x => x.Dispose());
    }

    public class Page
    {
        private readonly List<PageLine> _lines = new List<PageLine>();
        private readonly string _savePath;
        private readonly int _maxWidth;
        private readonly int _maxHeight;
        private int _height => _lines.Sum(x => x.Height);

        public Page(string savePath, int maxWidth, int maxHeight)
        {
            _savePath = savePath;
            _maxWidth = maxWidth;
            _maxHeight = maxHeight;
            _lines.Add(new PageLine(_maxWidth));
        }

        public bool CanAdd(ImageDetail detail)
        {
            if (_height + detail.Height <= _maxHeight)
                return true;
            if (detail.Height - _lines.Last().Height + _height > _maxHeight)
                return false;
            if (_lines.Last().CanAdd(detail))
                return true;
            return false;
        }

        public void Add(ImageDetail detail)
        {
            if (!_lines.Last().CanAdd(detail))
                _lines.Add(new PageLine(_maxWidth));
            _lines.Last().Add(detail);
        }

        public void Create()
        {
            var bitmap = new Bitmap(_maxWidth, _maxHeight);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                using (var brush = new SolidBrush(Color.White))
                    graphics.FillRectangle(brush, 0, 0, _maxWidth, _maxHeight);
                var offset = (_maxHeight - _height) / 2;
                _lines.ForEach(x =>
                {
                    x.Apply(graphics, offset);
                    offset += x.Height;
                });
                graphics.Flush();
                bitmap.Save(_savePath, ImageFormat.Png);
            }
        }
        
        public void Dispose() => _lines.ForEach(x => x.Dispose());
    }

    public class PageLine
    {
        private readonly List<ImageDetail> _details = new List<ImageDetail>();
        private readonly int _maxWidth;
        private int _width => _details.Sum(x => x.Width);
        public int Height => _details.Any() ? _details.Select(x => x.Height).Max() : 0;

        public PageLine(int maxWidth)
        {
            _maxWidth = maxWidth;
        }

        public bool CanAdd(ImageDetail detail) => _width + detail.Width <= _maxWidth;
        public void Add(ImageDetail detail) => _details.Add(detail);

        public void Apply(Graphics graphics, int y)
        {
            var offset = (_maxWidth - _width) / 2;
            _details.ForEach(x =>
            {
                x.Apply(graphics, offset, y);
                offset += x.Width;
            });
        }

        public void Dispose() => _details.ForEach(x => x.Dispose());
    }

    public class ImageDetail
    {
        private readonly Image _image;
        public int Width { get; }
        public int Height { get; }
        private readonly int _thickness;

        public ImageDetail(string basePath, string imagePath, int thickness)
        {
            _image = Image.FromFile(Path.Combine(basePath, imagePath));
            Width = _image.Width;
            Height = _image.Height;
            _thickness = thickness;
        }

        public void Apply(Graphics graphics, int x, int y)
        {
            var width = Width + (_thickness * 2);
            var height = Height + (_thickness * 2);
            using (var bitmap = new Bitmap(width, height))
            {
                using (var imageGraphics = Graphics.FromImage(bitmap))
                {
                    using (var brush = new SolidBrush(Color.Black))
                        imageGraphics.FillRectangle(brush, 0, 0, width, height);
                    imageGraphics.DrawImage(_image, new Rectangle(_thickness, _thickness, Width, Height));
                    graphics.DrawImage(bitmap, new Rectangle(x - _thickness, y - _thickness, Width + (_thickness * 2), Height + (_thickness * 2)));
                }
            }
        }

        public void Dispose() => _image.Dispose();
    }
}
