using System.Drawing;
using PiecePrinterPrep.Extensions;

namespace PiecePrinterPrep
{
    public class ImageToPrint
    {
        private readonly Image _front;
        private readonly Image _back;
        public int Count { get; }
        private int Trim { get; }
        public int Width { get; }
        public int Height { get; }

        public ImageToPrint(int count, int trim, string directory, string front, string back) 
            : this(count, trim, Image.FromFile(PathX.Build(directory, front)), Image.FromFile(PathX.Build(directory, back))) {}

        public ImageToPrint(int count, int trim, Image front, Image back)
        {
            Count = count;
            Trim = trim;
            _front = front;
            _back = back;
            Width = _front.Width - Trim * 2;
            Height = _back.Height - Trim * 2;
        }

        public void Apply(Graphics frontGraphics, Graphics backGraphics, int frontX, int frontY, int backX, int backY)
        {
            frontGraphics.DrawImage(_front, new Rectangle(frontX, frontY, Width, Height), new Rectangle(Trim, Trim, Width, Height), GraphicsUnit.Pixel);
            frontGraphics.DrawRectangle(new Pen(Color.Black, 10), frontX, frontY, Width, Height);
            backGraphics.DrawImage(_back, new Rectangle(backX, backY, Width, Height), new Rectangle(Trim, Trim, Width, Height), GraphicsUnit.Pixel);
        }

        public void Dispose()
        {
            _front.Dispose();
            _back.Dispose();
        }
    }
}
