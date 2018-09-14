using System.Collections.Generic;
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
        static void Main(string[] args)
        {
            var templatePath = args[0];
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
                var images = ((JArray)template["Images"]).Children<JObject>().Select(x => new ImageDetail(x, interpreter)).ToList();
                var borders = ((JArray)template["Borders"]).Children<JObject>().Select(x => new BorderDetail(x, interpreter)).ToList();
                var texts = ((JArray)template["Texts"]).Children<JObject>().Select(x => new TextDetail(x, interpreter)).ToList();

                var bitmap = new Bitmap(canvas.Width, canvas.Height);
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    using (var brush = new SolidBrush(canvas.Background))
                        graphics.FillRectangle(brush, 0, 0, canvas.Width, canvas.Height);
                    images.ForEach(x => x.Apply(graphics));
                    borders.ForEach(x => x.Apply(graphics));
                    texts.ForEach(x => x.Apply(graphics));
                    graphics.Flush();
                    bitmap.Save(Path.Combine(template.GetPropertyValue("Path"), item.GetPropertyValue("FileName")), ImageFormat.Png);
                }
            }
        }
    }
}
