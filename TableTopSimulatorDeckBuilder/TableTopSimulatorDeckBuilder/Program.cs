using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using TableTopSimulatorDeckBuilder.Extensions;

namespace TableTopSimulatorDeckBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            args.ForEach(Go);
            Console.Write("Success");
            Console.Read();
        }

        static void Go(string instructionsPath)
        {
            var instructionsDir = Path.GetDirectoryName(instructionsPath);
            var instructions = JObjectX.FromFile(instructionsPath);
            var savePath = instructions.GetPropertyValue("SavePath");
            var decks = ((JArray) instructions["Decks"]).ToObject<List<JObject>>();
            decks.ForEach(deck =>
            {
                var cards = new DirectoryInfo(PathX.Build(instructionsDir, deck.GetPropertyValue("Path"))).GetFiles("*.png").ToList();
                var sampleCard = Image.FromFile(cards[0].FullName);
                var width = sampleCard.Width;
                var height = sampleCard.Height;
                sampleCard.Dispose();
                var deckSheets = new List<List<FileInfo>>();
                for (int i = 0; i < cards.Count; i += 70)
                    deckSheets.Add(cards.GetRange(i, Math.Min(70, cards.Count - i)));

                for (var deckIndex = 0; deckIndex < deckSheets.Count; deckIndex++)
                {
                    var deckSheet = deckSheets[deckIndex];
                    var bitmap = new Bitmap(width * 10, height * 7);
                    bitmap.WithGraphics(g =>
                    {
                        for (var cardIndex = 0; cardIndex < deckSheet.Count; cardIndex++)
                            using (var cardImage = Image.FromFile(deckSheet[cardIndex].FullName))
                                g.DrawImage(cardImage, new Point(cardIndex % 10 * width, (int)Math.Floor((decimal)cardIndex / 10) * height));
                        g.Flush();
                        var result = deck.ContainsKey("Scale") ? ResizeImage(bitmap, (int)(bitmap.Width * (decimal)deck["Scale"]), (int)(bitmap.Height * (decimal)deck["Scale"])) : bitmap;
                        result.Save(PathX.Build(instructionsDir, savePath, deck.GetPropertyValue("Name") + deckIndex + ".png"), ImageFormat.Png);
                    });
                }
                SaveCardBack(width, height, instructionsDir, deck, savePath);
            });
        }

        private static void SaveCardBack(int width, int height, string instructionsDir, JObject deck, string savePath)
        {
            var cardBack = new Bitmap(width, height);
            cardBack.WithGraphics(g =>
            {
                g.DrawImage(Image.FromFile(Path.Combine(instructionsDir, deck.GetPropertyValue("CardBackPath"))),
                    new Rectangle(0, 0, width, height),
                    new Rectangle(
                        deck.ContainsKey("CardBackTrim") ? (int)deck["CardBackTrim"] : 0,
                        deck.ContainsKey("CardBackTrim") ? (int)deck["CardBackTrim"] : 0,
                        width,
                        height),
                    GraphicsUnit.Pixel);
                g.Flush();
                var result = deck.ContainsKey("Scale") ? ResizeImage(cardBack, (int)(cardBack.Width * (decimal)deck["Scale"]), (int)(cardBack.Height * (decimal)deck["Scale"])) : cardBack;
                result.Save(PathX.Build(instructionsDir, savePath, deck.GetPropertyValue("Name") + "Back" + ".png"),
                    ImageFormat.Png);
            });
        }

        private static Bitmap ResizeImage(Bitmap orginal, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(orginal.HorizontalResolution, orginal.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(orginal, destRect, 0, 0, orginal.Width, orginal.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
