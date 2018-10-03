using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Generator
{
    class Program
    {
        private static string _templateDir;
        static Dictionary<string, Action<JObject, CustomJInterpreter, Graphics>> _applyType = new Dictionary<string, Action<JObject, CustomJInterpreter, Graphics>>
        {
            { "Border", (obj, interpreter, graphics) => new BorderDetail(obj, interpreter).Apply(graphics) },
            { "Text", (obj, interpreter, graphics) => new TextDetail(obj, interpreter).Apply(graphics) },
            { "Image", (obj, interpreter, graphics) => new ImageDetail(_templateDir, obj, interpreter).Apply(graphics) },
            { "Rectangle", (obj, interpreter, graphics) => new RectangleDetail(obj, interpreter).Apply(graphics) },
        };

        static void Main(string[] args)
        {
            var templatePath = args[0];
            _templateDir = Path.GetDirectoryName(templatePath);
            var listPath = args[1];
            var templateJson = File.ReadAllText(templatePath);
            var listJson = File.ReadAllText(listPath);
            var template = JObject.Parse(templateJson);
            var list = JArray.Parse(listJson);
            var aliases = (JObject)template["Keywords"];
            foreach (var item in list.Children<JObject>())
            {
                var interpreter = new CustomJInterpreter(item, aliases);
                var canvas = new Canvas((JObject)template["Canvas"], interpreter);
                var items = ((JArray) template["Items"]).Children<JObject>().Where(x => interpreter.GetBoolOrDefault(x, "Enabled", true)).ToList();
                var bitmap = new Bitmap(canvas.Width, canvas.Height);
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    using (var brush = new SolidBrush(canvas.Background))
                        graphics.FillRectangle(brush, 0, 0, canvas.Width, canvas.Height);
                    items.ForEach(x => _applyType[x.GetPropertyValue("Type")](x, interpreter, graphics));
                    graphics.Flush();
                    bitmap.Save(Path.Combine(template.GetPropertyValue("Path"), item.GetPropertyValue("FileName")), ImageFormat.Png);
                }
            }
        }
    }
}
