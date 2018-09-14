using System.Drawing;
using Newtonsoft.Json.Linq;

namespace Generator
{
    public class BorderDetail
    {
        public Color Color;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public int Thickness;
        public BorderAlignment Alignment;

        public BorderDetail(JObject border, CustomJInterpreter interpreter)
        {
            Color = interpreter.GetColorOrDefault(border, "Color", Color.Black);
            X = interpreter.GetInt(border, "X");
            Y = interpreter.GetInt(border, "Y");
            Width = interpreter.GetInt(border, "Width");
            Height = interpreter.GetInt(border, "Height");
            Thickness = interpreter.GetInt(border, "Thickness");
            Alignment = interpreter.GetEnum<BorderAlignment>(border, "Alignment");
        }

        public void Apply(Graphics graphics)
        {
            var offset = (Thickness / 2) * (int)Alignment;
            graphics.DrawLine(new Pen(Color, Thickness), X + offset, Y, X + offset, Y + Height);
            graphics.DrawLine(new Pen(Color, Thickness), X, Y + Height - offset, X + Width, Y + Height - offset);
            graphics.DrawLine(new Pen(Color, Thickness), X + Width - offset, Y + Height, X + Width - offset, Y);
            graphics.DrawLine(new Pen(Color, Thickness), X + Width, Y + offset, X, Y + offset);
        }
    }
}
