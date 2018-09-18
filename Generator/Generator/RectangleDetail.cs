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

        public RectangleDetail(JObject border, CustomJInterpreter interpreter)
        {
            Color = interpreter.GetColorOrDefault(border, "Color", Color.Black);
            X = interpreter.GetInt(border, "X");
            Y = interpreter.GetInt(border, "Y");
            Width = interpreter.GetInt(border, "Width");
            Height = interpreter.GetInt(border, "Height");
        }

        public void Apply(Graphics graphics)
        {
            using (var brush = new SolidBrush(Color))
                graphics.FillRectangle(brush, X, Y, Width, Height);
        }
    }
}
