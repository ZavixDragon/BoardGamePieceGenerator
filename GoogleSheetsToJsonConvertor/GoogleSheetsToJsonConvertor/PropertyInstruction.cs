using System;
using System.Collections.Generic;
using GoogleSheetsToJsonConvertor.Extensions;
using GoogleSheetsToJsonConvertor.Modifications;
using Newtonsoft.Json.Linq;

namespace GoogleSheetsToJsonConvertor
{
    public class PropertyInstruction
    {
        private readonly Dictionary<string, Func<JObject, string, string>> _modifications = new Dictionary<string, Func<JObject, string, string>>
        {
            { "Replace", (mod, str) => new ReplaceModification(mod, str).Get() },
            { "Add", (mod, str) => new AddModification(mod, str).Get() }
        };
        private readonly JObject _instruction;
        private readonly int _columnIndex;

        public PropertyInstruction(JObject propertyInstruction)
        {
            _instruction = propertyInstruction;
            _columnIndex = (int)propertyInstruction["ColumnIndex"];
        }

        public void Apply(JObject result, List<string> item)
        {
            var property = _columnIndex >= item.Count ? "" : item[_columnIndex];
            if (_instruction.ContainsKey("Modifiers"))
                property = ApplyModifiers(property, ((JArray)_instruction["Modifiers"]).ToObject<List<JObject>>());
            result[_instruction.GetPropertyValue("Name")] = property;
        }

        private string ApplyModifiers(string property, List<JObject> modifiers)
        {
            var str = property;
            modifiers.ForEach(x => str = _modifications[x.GetPropertyValue("Type")](x, str));
            return str;
        }
    }
}
