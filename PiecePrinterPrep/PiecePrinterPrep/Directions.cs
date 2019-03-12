using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PiecePrinterPrep.Extensions;

namespace PiecePrinterPrep
{
    public class Directions
    {
        private readonly List<JObject> _directions;

        public Directions(List<JObject> directions)
        {
            _directions = directions;
        }

        public List<ImageToPrint> ExtractImages(ImageDetails details)
        {
            var result = new List<ImageToPrint>();
            _directions.ForEach(x =>
            {
                if (x.GetPropertyValue("Type") == "Set")
                    details.Set(x);
                else if (x.GetPropertyValue("Type") == "AddImages")
                {
                    details.SetupForAddImages(x);
                    result.AddRange(new Range((JObject)x["Range"]).ExtractImages(details));
                }
            });
            return result;
        }
    }
}
