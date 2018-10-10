using System.Drawing;
using System.Drawing.Printing;

namespace Printer
{
    class Program
    {
        private static Image _image;

        static void Main(string[] args)
        {
            foreach (var pagePath in args)
            {
                _image = Image.FromFile(pagePath);
                var print = new PrintDocument();
                print.PrintPage += PrintPage;
                print.Print();
                print.Dispose();
            }
        }

        private static void PrintPage(object o, PrintPageEventArgs e)
        {
            var width = (float)_image.Width / 3; //This assumes you are using 300 pixels per inch
            var height = (float)_image.Height / 3; //the equation goes divide by pixels per inch then multiply by 100
            e.Graphics.DrawImage(_image, 0, 0, width, height);
        }
    }
}
