using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace GoogleSheetsToJsonConvertor
{
    public class JObjectBuilder
    {
        private readonly List<PropertyInstruction> _instructions;

        public JObjectBuilder(List<PropertyInstruction> instructions)
        {
            _instructions = instructions;
        }

        public JObject Build(List<string> item)
        {
            var result = new JObject();
            _instructions.ForEach(x => x.Apply(result, item));
            return result;
        }
    }
}
