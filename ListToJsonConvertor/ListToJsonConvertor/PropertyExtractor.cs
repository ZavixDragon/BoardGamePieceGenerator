using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace ListToJsonConvertor
{
    public sealed class PropertyExtractor
    {
        private readonly List<string> _names = new List<string>();

        public PropertyExtractor(JObject instructions)
        {
            ((JArray)instructions["Properties"]).ForEach(x => _names.Add(((JObject)x).GetPropertyValue("Name")));
        }

        public string GetProperty(string name, List<string> item)
        {
            var property = new List<string>();
            var foundProperty = false;
            foreach (var line in item)
            {
                if (!foundProperty && IsProperty(name, line))
                {
                    foundProperty = true;
                    property.Add(line.Substring(name.Length).Trim());
                }
                else if (foundProperty)
                {
                    if (IsDifferentProperty(name, line))
                        break;
                    property.Add(line.Trim());
                }
            }
            if (property.Count == 0)
                throw new Exception($"Missing Property \"{name}\" on item: \n{string.Join(@"\n", item)}");
            return string.Join("\n", property);
        }
        
        private bool IsProperty(string name, string line) => line.StartsWith(name);
        private bool IsDifferentProperty(string name, string line) => _names.Where(x => x != name).Any(line.StartsWith);
    }
}