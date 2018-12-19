using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace ListToJsonConvertor
{
    public sealed class ReplaceModification
    {
        private readonly List<string> _patterns;
        private readonly string _replacement;
        private readonly string _property;

        public ReplaceModification(JObject replace, string property)
        {
            _patterns = ((JArray)replace["Patterns"]).ToObject<List<string>>();
            _replacement = replace.GetPropertyValue("Replacement");
            _property = property;
        }

        public string Get()
        {
            var str = _property;
            _patterns.ForEach(x => str = Regex.Replace(str, @x, _replacement));
            return str;
        }
    }
}