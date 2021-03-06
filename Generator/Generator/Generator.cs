using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Generator
{
    public class Generator
    {
        public void Run(string instructionsPath)
        {
            Console.WriteLine("");
            var instructionsDir = Path.GetDirectoryName(instructionsPath);
            var instructions = JObjectX.FromFile(instructionsPath);
            var savePath = instructions.GetPropertyValue("SavePath");
            var saveName = instructions.GetPropertyValue("SaveName");
            var items = GetItems(instructions, instructionsDir);
            var constants = GetConstants(instructions, instructionsDir);
            var prototypes = GetPrototypes(instructions, instructionsDir);
            var savePaths = new HashSet<string>();

            var applyType = new Dictionary<string, Action<JObject, CustomJPrototypeResolver, Graphics>>
            {
                { "Border", (obj, interpreter, graphics) => new BorderDetail(obj, interpreter).Apply(graphics) },
                { "Text", (obj, interpreter, graphics) => new TextDetail(instructionsDir, obj, interpreter).Apply(graphics) },
                { "Image", (obj, interpreter, graphics) => new ImageDetail(instructionsDir, obj, interpreter).Apply(graphics) },
                { "Rectangle", (obj, interpreter, graphics) => new RectangleDetail(obj, interpreter).Apply(graphics) },
                { "Circle", (obj, interpreter, graphics) => new CircleDetail(obj, interpreter).Apply(graphics) },
                { "CircleBorder", (obj, interpreter, graphics) => new CircleBorderDetail(obj, interpreter).Apply(graphics) },
            };

            for (var i = 0; i < items.Count; i++)
            {
                try
                {
                    var resolver =
                        new CustomJPrototypeResolver(prototypes, new CustomJInterpreter(items[i], constants));
                    var blueprint = (JObject) instructions["Blueprint"];
                    var canvas = new Canvas(blueprint, resolver);
                    var elements = ((JArray) blueprint["Elements"]).Children<JObject>()
                        .Where(x => resolver.GetBoolOrDefault(x, "Enabled", true)).ToList();
                    var bitmap = new Bitmap(canvas.Width, canvas.Height);
                    WithGraphics(bitmap, graphics =>
                    {
                        using (var brush = new SolidBrush(canvas.Background))
                            graphics.FillRectangle(brush, 0, 0, canvas.Width, canvas.Height);
                        elements.ForEach(x =>
                        {
                            try
                            {
                                applyType[resolver.GetString(x, "Type")](x, resolver, graphics);
                            }
                            catch
                            {
                                Console.WriteLine($"Error Adding Element: {x.ToString(Formatting.None)}");
                                throw;
                            }
                        });
                        graphics.Flush();
                        SaveTrimmedImage(resolver, instructions, blueprint, bitmap, instructionsDir, savePath, saveName, i, canvas, savePaths);
                    });
                }
                catch
                {
                    Console.WriteLine($"Error On Item {i}: {items[i].ToString(Formatting.None)}");
                    Console.Read();
                    throw;
                }
            }
        }

        private void SaveTrimmedImage(CustomJPrototypeResolver resolver, JObject instructions, JObject blueprint, Bitmap bitmap, string instructionsDir, string savePath, string saveName, int item, Canvas canvas, HashSet<string> savedPaths)
        {
            var trim = resolver.GetIntOrDefault(blueprint, "PostTrim", 0);
            var path = GetSavePath(resolver, instructions, blueprint, instructionsDir, savePath, saveName, item, savedPaths);
            if (trim == 0)
                bitmap.Save(path, ImageFormat.Png);
            else
            {
                var trimmedBitmap = new Bitmap(canvas.Width - trim * 2, canvas.Height - trim * 2);
                WithGraphics(trimmedBitmap, g =>
                {
                    g.DrawImage(bitmap,
                        new Rectangle(0, 0, trimmedBitmap.Width, trimmedBitmap.Height),
                        new Rectangle(trim, trim, trimmedBitmap.Width, trimmedBitmap.Height), GraphicsUnit.Pixel);
                    trimmedBitmap.Save(path, ImageFormat.Png);
                });
            }
        }

        private string GetSavePath(CustomJPrototypeResolver resolver, JObject instructions, JObject blueprint, string instructionsDir, string savePath, string saveName, int item, HashSet<string> savedPaths)
        {
            var extension = resolver.GetStringOrDefault(blueprint, "SavePathExtension", "");
            var path = saveName.Contains("~")
                ? PathX.Build(instructionsDir, savePath + extension, $"{resolver.GetString(instructions, "SaveName")}.png")
                : PathX.Build(instructionsDir, savePath + extension, $"{saveName}{item}.png");
            if (savedPaths.Contains(path))
            {
                Console.WriteLine($"Duplicate Save Path Detected: {path}");
                throw new ArgumentException();
            }
            savedPaths.Add(path);
            return path;
        }

        private List<JObject> GetItems(JObject instructions, string instructionsDir)
        {
            var items = instructions.ContainsKey("Items") ? instructions["Items"].Children<JObject>().ToList() : new List<JObject>();
            if (instructions.ContainsKey("ImportedItems"))
                foreach (var itemsFileName in instructions["ImportedItems"].Children<JToken>())
                    items.AddRange(JObjectX.FromFile(instructionsDir, itemsFileName.GetValue())["Items"].Children<JObject>());
            return items;
        }

        private JObject GetConstants(JObject instructions, string instructionsDir)
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

        private JObject GetPrototypes(JObject instructions, string instructionsDir)
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

        private void WithGraphics(Bitmap bitmap, Action<Graphics> action)
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
