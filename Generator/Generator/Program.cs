using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            var templatePath = args[0];
            var listPath = args[1];
            var templateJson = File.ReadAllText(templatePath);
            var listJson = File.ReadAllText(templatePath);
            var template = JObject.Parse(templateJson);
            var list = JArray.Parse(listJson);
            foreach (var item in list.Children<JObject>())
            {
                var bitmap = new Bitmap
            }
        }
    }
}
