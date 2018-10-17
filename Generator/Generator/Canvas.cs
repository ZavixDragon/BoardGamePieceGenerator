using System.Drawing;
using Newtonsoft.Json.Linq;

namespace Generator
{
    public class Canvas
    {
        public int Height;
        public int Width;
        public Color Background;

        public Canvas(JObject canvas, CustomJPrototypeResolver resolver)
        {
            Height = resolver.GetInt(canvas, "Height");
            Width = resolver.GetInt(canvas, "Width");
            Background = resolver.GetColorOrDefault(canvas, "Background", Color.White);
        }
    }
}
