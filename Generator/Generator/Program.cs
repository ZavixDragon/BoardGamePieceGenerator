using System;
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
            var instructionsPath = args[0];
            var instructionsDir = Path.GetDirectoryName(instructionsPath);
            var instructions = JObjectX.FromFile(instructionsPath);
            var savePath = instructions.GetPropertyValue("SavePath");
            var saveName = instructions.GetPropertyValue("SaveName");
            var items = GetItems(instructions, instructionsDir);
            var constants = GetConstants(instructions, instructionsDir);
            var prototypes = GetPrototypes(instructions, instructionsDir);

            var applyType = new Dictionary<string, Action<JObject, CustomJPrototypeResolver, Graphics>>
            {
                { "Border", (obj, interpreter, graphics) => new BorderDetail(obj, interpreter).Apply(graphics) },
                { "Text", (obj, interpreter, graphics) => new TextDetail(instructionsDir, obj, interpreter).Apply(graphics) },
                { "Image", (obj, interpreter, graphics) => new ImageDetail(instructionsDir, obj, interpreter).Apply(graphics) },
                { "Rectangle", (obj, interpreter, graphics) => new RectangleDetail(obj, interpreter).Apply(graphics) },
            };

            for (var i = 0; i < items.Count; i++)
            {
                var resolver = new CustomJPrototypeResolver(prototypes, new CustomJInterpreter(items[i], constants));
                var blueprint = (JObject) instructions["Blueprint"];
                var canvas = new Canvas(blueprint, resolver);
                var elements = ((JArray)blueprint["Elements"]).Children<JObject>().Where(x => resolver.GetBoolOrDefault(x, "Enabled", true)).ToList();
                var bitmap = new Bitmap(canvas.Width, canvas.Height);
                WithGraphics(bitmap, graphics =>
                {
                    using (var brush = new SolidBrush(canvas.Background))
                        graphics.FillRectangle(brush, 0, 0, canvas.Width, canvas.Height);
                    elements.ForEach(x => applyType[resolver.GetString(x, "Type")](x, resolver, graphics));
                    graphics.Flush();
                    bitmap.Save(PathX.Build(instructionsDir, savePath, $"{saveName}{i}.png"), ImageFormat.Png);
                });
            }
        }

        private static List<JObject> GetItems(JObject instructions, string instructionsDir)
        {
            var items = instructions.ContainsKey("Items") ? instructions["Items"].Children<JObject>().ToList() : new List<JObject>();
            if (instructions.ContainsKey("ImportedItems"))
                foreach (var itemsFileName in instructions["ImportedItems"].Children<JToken>())
                    items.AddRange(JObjectX.FromFile(instructionsDir, itemsFileName.GetValue())["Items"].Children<JObject>());
            return items;
        }

        private static JObject GetConstants(JObject instructions, string instructionsDir)
        {
            var constants = instructions.ContainsKey("Constants") ? (JObject) instructions["Constants"] : new JObject();
            if (instructions.ContainsKey("ImportedConstants"))
                foreach (var constantsFileName in instructions["ImportedConstants"].Children<JToken>())
                {
                    var importedConstants = (JObject) JObjectX.FromFile(instructionsDir, constantsFileName.GetValue())["Constants"];
                    importedConstants.Merge(constants);
                    constants = importedConstants;
                }
            return constants;
        }

        private static JObject GetPrototypes(JObject instructions, string instructionsDir)
        {
            var prototypes = instructions.ContainsKey("Prototypes") ? (JObject) instructions["Prototypes"] : new JObject();
            if (instructions.ContainsKey("ImportedPrototypes"))
                foreach (var prototypesFileName in instructions["ImportedPrototypes"].Children<JToken>())
                {
                    var importedPrototypes = (JObject)JObjectX.FromFile(instructionsDir, prototypesFileName.GetValue())["Prototypes"];
                    importedPrototypes.Merge(prototypes);
                    prototypes = importedPrototypes;
                }
            return prototypes;
        }

        private static void WithGraphics(Bitmap bitmap, Action<Graphics> action)
        {
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                action(graphics);
            }
        }
    }
}
