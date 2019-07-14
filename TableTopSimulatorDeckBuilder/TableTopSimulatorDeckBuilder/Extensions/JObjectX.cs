using System.IO;
using Newtonsoft.Json.Linq;

namespace TableTopSimulatorDeckBuilder.Extensions
{
    public static class JObjectX
    {
        public static JObject FromFile(params string[] pathSegments) => FromFile(PathX.Build(pathSegments));
        public static JObject FromFile(string path) => JObject.Parse(File.ReadAllText(path));
    }
}
