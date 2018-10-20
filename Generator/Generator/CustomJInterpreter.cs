using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace Generator
{
    public sealed class CustomJInterpreter
    {
        private readonly JObject _itemContext;
        private readonly JObject _constants;
        private readonly DataTable _mathResolver = new DataTable();

        public CustomJInterpreter(JObject itemContext, JObject constants)
        {
            _itemContext = itemContext;
            _constants = constants;
        }

        public string GetStringOrDefault(JObject baseObj, string key, string defaultValue) 
            => GetStringOrDefault(baseObj, key, () => defaultValue);

        public string GetStringOrDefault(JObject baseObj, string key, Func<string> defaultValue) 
            => baseObj.ContainsKey(key) ? GetString(baseObj, key) : defaultValue();
        
        public string GetString(JObject baseObj, string key) 
            => GetRootValue(baseObj.GetPropertyValue(key));

        public int GetIntOrDefault(JObject baseObj, string key, int defaultValue)
            => GetIntOrDefault(baseObj, key, () => defaultValue);
        
        public int GetIntOrDefault(JObject baseObj, string key, Func<int> defaultValue) 
            => baseObj.ContainsKey(key) ? GetInt(baseObj, key) : defaultValue();

        public int GetInt(JObject baseObj, string key) 
            => int.Parse(GetRootValue(baseObj.GetPropertyValue(key)));

        public decimal GetDecimalOrDefault(JObject baseObj, string key, decimal defaultValue)
            => GetDecimalOrDefault(baseObj, key, () => defaultValue);
        
        public decimal GetDecimalOrDefault(JObject baseObj, string key, Func<decimal> defaultValue) 
            => baseObj.ContainsKey(key) ? GetDecimal(baseObj, key) : defaultValue();

        public decimal GetDecimal(JObject baseObj, string key) 
            => decimal.Parse(GetRootValue(baseObj.GetPropertyValue(key)));

        public bool GetBoolOrDefault(JObject baseObj, string key, bool defaultValue)
            => GetBoolOrDefault(baseObj, key, () => defaultValue);
        
        public bool GetBoolOrDefault(JObject baseObj, string key, Func<bool> defaultValue) 
            => baseObj.ContainsKey(key) ? GetBool(baseObj, key) : defaultValue();

        public bool GetBool(JObject baseObj, string key)
            => bool.Parse(GetRootValue(baseObj.GetPropertyValue(key)));

        public Color GetColorOrDefault(JObject baseObj, string key, Color defaultValue)
            => GetColorOrDefault(baseObj, key, () => defaultValue);
        
        public Color GetColorOrDefault(JObject baseObj, string key, Func<Color> defaultValue)
            => baseObj.ContainsKey(key) ? GetColor(baseObj, key) : defaultValue();

        public Color GetColor(JObject baseObj, string key)
        {
            var rootValue = GetRootValue(baseObj.GetPropertyValue(key));
            return Color.FromArgb(rootValue.Length == 8 ? int.Parse(rootValue.Substring(6, 2), NumberStyles.HexNumber): 255, 
                int.Parse(rootValue.Substring(0, 2), NumberStyles.HexNumber),
                int.Parse(rootValue.Substring(2, 2), NumberStyles.HexNumber),
                int.Parse(rootValue.Substring(4, 2), NumberStyles.HexNumber));
        }

        public T GetEnumOrDefault<T>(JObject baseObj, string key, T defaultValue)
            => GetEnumOrDefault(baseObj, key, () => defaultValue);
        
        public T GetEnumOrDefault<T>(JObject baseObj, string key, Func<T> defaultValue)
            => baseObj.ContainsKey(key) ? GetEnum<T>(baseObj, key) : defaultValue();

        public T GetEnum<T>(JObject baseObj, string key)
            => (T)Enum.Parse(typeof(T), GetRootValue(baseObj.GetPropertyValue(key)));

        public T GetFlagsEnumOrDefault<T>(JObject baseObj, string key, T defaultValue)
            => GetFlagsEnumOrDefault(baseObj, key, () => defaultValue);
        
        public T GetFlagsEnumOrDefault<T>(JObject baseObj, string key, Func<T> defaultValue)
            => baseObj.ContainsKey(key) ? GetFlagsEnum<T>(baseObj, key) : defaultValue();

        public T GetFlagsEnum<T>(JObject baseObj, string key)
            => (T)(object)GetInt(baseObj, key);

        public string GetRootValue(string str)
        {
            while (str.Contains("{"))
            {
                var startOfSegment = str.IndexOf("{");
                var segment = "";
                var depth = 1;
                var strRemainder = "";
                for (var i = startOfSegment + 1; depth != 0; i++)
                {
                    if (str[i] == '{')
                        depth++;
                    else if (str[i] == '}')
                        depth--;
                    if (depth != 0)
                        segment += str[i];
                    else if (depth == 0)
                        strRemainder = str.Length == i + 1 ? "" : str.Substring(i + 1, str.Length - (i + 1));
                }
                str = $"{str.Substring(0, startOfSegment)}{GetRootValue(segment)}{strRemainder}";
            }
            if (str.StartsWith("~"))
                return GetRootValue(_itemContext.GetPropertyValue(GetRootValue(str.Substring(1))));
            if (str.StartsWith("^"))
                return GetRootValue(_constants.GetPropertyValue(GetRootValue(str.Substring(1))));
            if (str.StartsWith("#"))
                return _mathResolver.Compute(GetRootValue(str.Substring(1)), "").ToString();
            return str;
        }
    }
}
