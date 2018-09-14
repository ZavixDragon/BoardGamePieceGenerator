using System.Drawing;
using Newtonsoft.Json.Linq;

namespace Generator
{
    public class Canvas
    {
        public int Height;
        public int Width;
        public Color Background;

        public Canvas(JObject canvas, CustomJInterpreter interpreter)
        {
            Height = interpreter.GetInt(canvas, "Height");
            Width = interpreter.GetInt(canvas, "Width");
            Background = interpreter.GetColorOrDefault(canvas, "Background", Color.White);
        }
    }
}
