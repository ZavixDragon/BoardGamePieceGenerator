using Newtonsoft.Json.Linq;

namespace Generator
{
    public static class JObjectExtensions
    {
        public static string GetPropertyValue(this JObject jobj, string key) => (string)jobj[key];
        public static string GetValue(this JProperty jobj) => (string)jobj.Value;
        public static string GetValue(this JValue jobj) => (string)jobj;
        public static string GetValue(this JToken jobj) => (string)jobj;
    }
}
