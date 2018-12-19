using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace ListToJsonConvertor
{
    public sealed class PropertyInstruction
    {
        private readonly Dictionary<string, Func<JObject, string, string>> _modifications = new Dictionary<string, Func<JObject, string, string>>
        {
            { "Replace", (mod, str) => new ReplaceModification(mod, str).Get() },
            { "Add", (mod, str) => new AddModification(mod, str).Get() }
        };
        private readonly PropertyExtractor _extractor;
        private readonly JObject _instruction;
        private readonly string _name;

        public PropertyInstruction(PropertyExtractor extractor, JObject propertyInstruction)
        {
            _extractor = extractor;
            _instruction = propertyInstruction;
            _name = (string)propertyInstruction["Name"];
        }
        
        public void Apply(JObject result, List<string> item)
        {
            var property = _extractor.GetProperty(_name, item);
            if (_instruction.ContainsKey("Skip") && Regex.IsMatch(property, @_instruction.GetPropertyValue("Skip")))
                return;
            if (_instruction.ContainsKey("Blank") && Regex.IsMatch(property, @_instruction.GetPropertyValue("Blank")))
                property = "";
            else if (_instruction.ContainsKey("Modifiers"))
                property = ApplyModifiers(property, ((JArray)_instruction["Modifiers"]).ToObject<List<JObject>>());
            result[_instruction.ContainsKey("OutputName") ? _instruction.GetPropertyValue("OutputName") : _name] = property;
        }

        private string ApplyModifiers(string property, List<JObject> modifiers)
        {
            var str = property;
            modifiers.ForEach(x => str = _modifications[x.GetPropertyValue("Type")](x, str));
            return str;
        }
    }
}