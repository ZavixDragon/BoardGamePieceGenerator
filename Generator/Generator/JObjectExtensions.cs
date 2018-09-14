using Newtonsoft.Json.Linq;

namespace Generator
{
    public static class JObjectExtensions
    {
        public static string GetPropertyValue(this JObject jobj, string key) => (string)jobj[key];
    }
}
