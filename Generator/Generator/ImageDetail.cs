using System.Drawing;
using Newtonsoft.Json.Linq;

namespace Generator
{
    public class ImageDetail
    {
        public string Source;
        public decimal Opacity;
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public ImageDetail(JObject image, CustomJInterpreter interpreter)
        {
            Source = interpreter.GetString(image, "Source");
            Opacity = interpreter.GetDecimalOrDefault(image, "Opacity", 1);
            X = interpreter.GetInt(image, "X");
            Y = interpreter.GetInt(image, "Y");
            Width = interpreter.GetInt(image, "Width");
            Height = interpreter.GetInt(image, "Height");
        }

        public void Apply(Graphics graphics)
        {
            var img = Image.FromFile(Source).WithOpacity(Opacity);
            graphics.DrawImage(img, new Rectangle(X, Y, Width, Height), new Rectangle(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);
        }
    }
}
