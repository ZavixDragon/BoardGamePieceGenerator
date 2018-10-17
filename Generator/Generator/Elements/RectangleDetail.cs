using System.Drawing;
using Newtonsoft.Json.Linq;

namespace Generator
{
    public class RectangleDetail
    {
        public Color Color;
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public RectangleDetail(JObject border, CustomJPrototypeResolver resolver)
        {
            Color = resolver.GetColorOrDefault(border, "Color", Color.Black);
            X = resolver.GetInt(border, "X");
            Y = resolver.GetInt(border, "Y");
            Width = resolver.GetInt(border, "Width");
            Height = resolver.GetInt(border, "Height");
        }

        public void Apply(Graphics graphics)
        {
            using (var brush = new SolidBrush(Color))
                graphics.FillRectangle(brush, X, Y, Width, Height);
        }
    }
}
