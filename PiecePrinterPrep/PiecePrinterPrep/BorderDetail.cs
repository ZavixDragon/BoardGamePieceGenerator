using System.Drawing;

namespace PiecePrinterPrep
{
    public class BorderDetail
    {
        public Color Color;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public int Thickness;

        public BorderDetail(Color color, int x, int y, int width, int height, int thickness)
        {
            Color = color;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Thickness = thickness;
        }

        public void Apply(Graphics graphics)
        {
            var offset = Thickness / 2;
            graphics.DrawLine(new Pen(Color, Thickness), X + offset, Y, X + offset, Y + Height);
            graphics.DrawLine(new Pen(Color, Thickness), X, Y + Height - offset, X + Width, Y + Height - offset);
            graphics.DrawLine(new Pen(Color, Thickness), X + Width - offset, Y + Height, X + Width - offset, Y);
            graphics.DrawLine(new Pen(Color, Thickness), X + Width, Y + offset, X, Y + offset);
        }
    }
}