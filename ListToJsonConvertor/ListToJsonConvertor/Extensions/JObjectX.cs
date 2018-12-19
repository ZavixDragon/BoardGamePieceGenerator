using System.IO;
using Newtonsoft.Json.Linq;

namespace ListToJsonConvertor
{
    public static class JObjectX
    {
        public static JObject FromFile(params string[] pathSegments) => FromFile(PathX.Build(pathSegments));
        public static JObject FromFile(string path) => JObject.Parse(File.ReadAllText(path));
    }
}