using System;
using System.Drawing;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace Generator
{
    public sealed class CustomJInterpreter
    {
        private readonly JObject _itemContext;
        private readonly JObject _keywordMap;

        public CustomJInterpreter(JObject itemContext, JObject keywordMap)
        {
            _itemContext = itemContext;
            _keywordMap = keywordMap;
        }

        public string GetStringOrDefault(JObject baseObj, string key, string defaultValue)
        {
            return baseObj.ContainsKey(key) ? GetString(baseObj, key) : defaultValue;
        }

        public string GetString(JObject baseObj, string key)
        {
            return GetRootValue(baseObj.GetPropertyValue(key));
        }

        public int GetIntOrDefault(JObject baseObj, string key, int defaultValue)
        {
            return baseObj.ContainsKey(key) ? GetInt(baseObj, key) : defaultValue;
        }

        public int GetInt(JObject baseObj, string key)
        {
            return int.Parse(GetRootValue(baseObj.GetPropertyValue(key)));
        }

        public decimal GetDecimalOrDefault(JObject baseObj, string key, decimal defaultValue)
        {
            return baseObj.ContainsKey(key) ? GetDecimal(baseObj, key) : defaultValue;
        }

        public decimal GetDecimal(JObject baseObj, string key)
        {
            return decimal.Parse(GetRootValue(baseObj.GetPropertyValue(key)));
        }

        public bool GetBoolOrDefault(JObject baseObj, string key, bool defaultValue)
        {
            return baseObj.ContainsKey(key) ? GetBool(baseObj, key) : defaultValue;
        }

        public bool GetBool(JObject baseObj, string key)
        {
            return bool.Parse(GetRootValue(baseObj.GetPropertyValue(key)));
        }

        public Color GetColorOrDefault(JObject baseObj, string key, Color defaultValue)
        {
            return baseObj.ContainsKey(key) ? GetColor(baseObj, key) : defaultValue;
        }

        public Color GetColor(JObject baseObj, string key)
        {
            var rootValue = GetRootValue(baseObj.GetPropertyValue(key));
            return Color.FromArgb(255, int.Parse(rootValue.Substring(0, 2), NumberStyles.HexNumber),
                int.Parse(rootValue.Substring(2, 2), NumberStyles.HexNumber),
                int.Parse(rootValue.Substring(4, 2), NumberStyles.HexNumber));
        }

        public T GetEnumOrDefault<T>(JObject baseObj, string key, T defaultValue)
        {
            return baseObj.ContainsKey(key) ? GetEnum<T>(baseObj, key) : defaultValue;
        }

        public T GetEnum<T>(JObject baseObj, string key)
        {
            return (T)Enum.Parse(typeof(T), GetRootValue(baseObj.GetPropertyValue(key)));
        }

        public T GetFlagsEnumOrDefault<T>(JObject baseObj, string key, T defaultValue)
        {
            return baseObj.ContainsKey(key) ? GetFlagsEnum<T>(baseObj, key) : defaultValue;
        }

        public T GetFlagsEnum<T>(JObject baseObj, string key)
        {
            return (T)(object)GetInt(baseObj, key);
        }

        public string GetRootValue(string str)
        {
            if (str.StartsWith("*"))
                return GetRootValue(_itemContext.GetPropertyValue(GetRootValue(str.Substring(1))));
            if (str.StartsWith("^"))
                return GetRootValue(_keywordMap.GetPropertyValue(GetRootValue(str.Substring(1))));
            return str;
        }
    }
}
