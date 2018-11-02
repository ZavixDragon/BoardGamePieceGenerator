using System.Drawing;
using Newtonsoft.Json.Linq;

namespace Generator
{
    public class CircleBorderDetail
    {
        public Color Color;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public int Thickness;
        public BorderAlignment Alignment;

        public CircleBorderDetail(JObject border, CustomJPrototypeResolver resolver)
        {
            Color = resolver.GetColorOrDefault(border, "Color", Color.Black);
            X = resolver.GetInt(border, "X");
            Y = resolver.GetInt(border, "Y");
            Width = resolver.GetInt(border, "Width");
            Height = resolver.GetInt(border, "Height");
            Thickness = resolver.GetInt(border, "Thickness");
            Alignment = resolver.GetEnum<BorderAlignment>(border, "Alignment");
        }

        public void Apply(Graphics graphics)
        {
            var offset = (Thickness / 2) * (int)Alignment;
            using (var pen = new Pen(Color, Thickness))
                graphics.DrawEllipse(pen, X + offset, Y + offset, Width - 2 * offset, Height - 2 * offset);
        }
    }
}
