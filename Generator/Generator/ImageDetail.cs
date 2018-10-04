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

        public ImageDetail(string templateDir, JObject image, CustomJInterpreter interpreter)
        {
            _templateDir = templateDir;
            Source = interpreter.GetString(image, "Source");
            Opacity = interpreter.GetDecimalOrDefault(image, "Opacity", 1);
            X = interpreter.GetInt(image, "X");
            Y = interpreter.GetInt(image, "Y");
            Width = interpreter.GetInt(image, "Width");
            Height = interpreter.GetInt(image, "Height");
        }

        public void Apply(Graphics graphics)
        {
            var img = Image.FromFile(Path.GetFullPath(Path.Combine(_templateDir, Source))).WithOpacity(Opacity);
            graphics.DrawImage(img, new Rectangle(X, Y, Width, Height), new Rectangle(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);
        }
    }
}
