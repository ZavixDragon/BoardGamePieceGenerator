using System.Collections.Generic;
using System.Linq;

namespace ListToJsonConvertor
{
    public sealed class SeparatedItems
    {
        private readonly List<string> _lines;
        
        public SeparatedItems(List<string> lines)
        {
            _lines = lines;
        }

        public List<List<string>> Get()
        {
            var items = new List<List<string>>();
            foreach (var line in _lines)
            {
                if (line.StartsWith("Name:"))
                {
                    if (items.Count != 0)
                        items.Last().Remove(items.Last().Last());
                    items.Add(new List<string>());
                }
                items.Last().Add(line);
            }
            return items.Where(x => x.Count > 0).ToList();
        }
    }
}