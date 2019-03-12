using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using PiecePrinterPrep.Extensions;

namespace PiecePrinterPrep
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
            var directions = new Directions(((JArray) instructions["Images"]).ToObject<List<JObject>>());
            var imagesToPrint = directions.ExtractImages(new ImageDetails(instructionsDir));
            var pageNumber = 1;
            var pages = new List<Page> { new Page(1) };
            imagesToPrint.ForEach(x => Enumerable.Range(0, x.Count).ForEach(_ =>
            {
                if (!pages.Last().CanAdd(x))
                    pages.Add(new Page(++pageNumber));
                pages.Last().Add(x);
            }));
            pages.ForEach(x => x.Create(PathX.Build(instructionsDir, instructions.GetPropertyValue("SavePath")), instructions.GetPropertyValue("SaveName")));
            imagesToPrint.ForEach(x => x.Dispose());
        }
    }
}
