using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace ListToJsonConvertor
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var instructionsPath = args[0];
            var instructionsDir = Path.GetDirectoryName(instructionsPath);
            var instructions = JObjectX.FromFile(instructionsPath);

            var propertyExtractor = new PropertyExtractor(instructions);
            var propertyInstructions = ((JArray) instructions["Properties"]).Select(x => new PropertyInstruction(propertyExtractor, (JObject)x)).ToList();
            var itemProcessor = new JObjectExtractor(propertyInstructions);
            
            var lines = File.ReadLines(PathX.Build(instructionsDir, instructions.GetPropertyValue("Input"))).ToList();
            var unprocessedItems = new SeparatedItems(lines).Get();
            var items = unprocessedItems.Select(x => itemProcessor.Extract(x)).ToList();
            
            var jArray = new JArray(items);
            dynamic jObject = new JObject();
            jObject.Items = jArray;
            File.WriteAllText(PathX.Build(instructionsDir, instructions.GetPropertyValue("Output")), jObject.ToString());
        }
    }
}