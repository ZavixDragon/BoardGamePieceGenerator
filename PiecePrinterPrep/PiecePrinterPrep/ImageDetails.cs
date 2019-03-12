using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PiecePrinterPrep.Extensions;

namespace PiecePrinterPrep
{
    public class ImageDetails
    {
        private readonly Dictionary<string, string> _tempValues = new Dictionary<string, string>();
        private int _count;
        private int _trim;
        private string _front;
        private string _back;
        public string Directory { get; }
        public int Count => _tempValues.ContainsKey("Count") ? int.Parse(_tempValues["Count"]) : _count;
        public int Trim => _tempValues.ContainsKey("Trim") ? int.Parse(_tempValues["Trim"]) : _trim;
        public string Front => _tempValues.ContainsKey("Front") ? _tempValues["Front"] : _front;
        public string Back => _tempValues.ContainsKey("Back") ? _tempValues["Back"] : _back;

        public ImageDetails(string directory)
        {
            Directory = directory;
        }

        public void Set(JObject setDirection)
        {
            var key = setDirection.GetPropertyValue("Key");
            if (key == "Count")
                _count = int.Parse(setDirection.GetPropertyValue("Value"));
            else if (key == "Trim")
                _trim = int.Parse(setDirection.GetPropertyValue("Value"));
            else if (key == "Front")
                _front = setDirection.GetPropertyValue("Value");
            else if (key == "Back")
                _back = setDirection.GetPropertyValue("Value");
            else
                throw new ArgumentException("Set direction has an invalid key: " + key);
        }

        public void SetupForAddImages(JObject imageDirection)
        {
            _tempValues.Clear();
            if (imageDirection.ContainsKey("Count"))
                _tempValues["Count"] = imageDirection.GetPropertyValue("Count");
            if (imageDirection.ContainsKey("Trim"))
                _tempValues["Trim"] = imageDirection.GetPropertyValue("Trim");
            if (imageDirection.ContainsKey("Front"))
                _tempValues["Front"] = imageDirection.GetPropertyValue("Front");
            if (imageDirection.ContainsKey("Back"))
                _tempValues["Back"] = imageDirection.GetPropertyValue("Back");
        }
    }
}
