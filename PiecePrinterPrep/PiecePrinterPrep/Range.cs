using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using PiecePrinterPrep.Extensions;

namespace PiecePrinterPrep
{
    public class Range
    {
        private readonly Dictionary<string, Func<ImageDetails, JObject, List<ImageToPrint>>> _rangeTypes = new Dictionary<string, Func<ImageDetails, JObject, List<ImageToPrint>>>
        {
            { "Single", (details, range) => new List<ImageToPrint> { new ImageToPrint(details.Count, details.Trim, details.Directory, details.Front, details.Back) } },
            { "ListFront", (details, range) => ((JArray)range["List"]).ToObject<List<string>>().Select(x => new ImageToPrint(
                details.Count,
                details.Trim, 
                details.Directory, 
                $"{details.Front.Substring(0, details.Front.LastIndexOf('.'))}{x}{details.Front.Substring(details.Front.LastIndexOf('.'))}", 
                details.Back)).ToList() },
            { "ListBack", (details, range) => ((JArray)range["List"]).ToObject<List<string>>().Select(x => new ImageToPrint(
                details.Count,
                details.Trim,
                details.Directory,
                details.Front,
                $"{details.Back.Substring(0, details.Back.LastIndexOf('.'))}{x}{details.Back.Substring(details.Back.LastIndexOf('.'))}")).ToList() },
            { "List", (details, range) => ((JArray)range["List"]).ToObject<List<JObject>>().Select(x => new ImageToPrint(
                details.Count,
                details.Trim,
                details.Directory,
                $"{details.Front.Substring(0, details.Front.LastIndexOf('.'))}{x.GetPropertyValue("Front")}{details.Front.Substring(details.Front.LastIndexOf('.'))}",
                $"{details.Back.Substring(0, details.Back.LastIndexOf('.'))}{x.GetPropertyValue("Back")}{details.Back.Substring(details.Back.LastIndexOf('.'))}")).ToList() },
            { "Range", (details, range) =>
                {
                    var innerRange = (JObject) range["Range"];
                    var start = (int) innerRange["Start"];
                    var end = (int) innerRange["End"];
                    return Enumerable.Range(start, end - start + 1).Select(x => new ImageToPrint(
                        details.Count,
                        details.Trim,
                        details.Directory,
                        $"{details.Front.Substring(0, details.Front.LastIndexOf('.'))}{x}{details.Front.Substring(details.Front.LastIndexOf('.'))}",
                        details.Back)).ToList();
                }
            }
        };
        private readonly JObject _range;

        public Range(JObject range)
        {
            _range = range;
        }

        public List<ImageToPrint> ExtractImages(ImageDetails details) =>
            _rangeTypes[_range.GetPropertyValue("Type")](details, _range);
    }
}
