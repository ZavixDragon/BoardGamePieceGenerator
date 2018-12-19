using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ListToJsonConvertor
{
    public sealed class JObjectExtractor
    {
        private readonly List<PropertyInstruction> _instructions;

        public JObjectExtractor(List<PropertyInstruction> instructions)
        {
            _instructions = instructions;
        }

        public JObject Extract(List<string> item)
        {
            var result = new JObject();
            _instructions.ForEach(x => x.Apply(result, item));
            return result;
        }
    }
}