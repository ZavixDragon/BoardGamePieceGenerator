using System.Drawing;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Generator
{
    public class ImageDetail
    {
        private readonly string _templateDir;
        public string Source;
        public decimal Opacity;
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public ImageDetail(string templateDir, JObject image, CustomJPrototypeResolver resolver)
        {
            _templateDir = templateDir;
            Source = resolver.GetString(image, "Source");
            Opacity = resolver.GetDecimalOrDefault(image, "Opacity", 1);
            X = resolver.GetInt(image, "X");
            Y = resolver.GetInt(image, "Y");
            Width = resolver.GetInt(image, "Width");
            Height = resolver.GetInt(image, "Height");
        }

        public void Apply(Graphics graphics)
        {
            var img = Image.FromFile(Path.GetFullPath(Path.Combine(_templateDir, Source))).WithOpacity(Opacity);
            graphics.DrawImage(img, new Rectangle(X, Y, Width, Height), new Rectangle(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);
        }
    }
}
