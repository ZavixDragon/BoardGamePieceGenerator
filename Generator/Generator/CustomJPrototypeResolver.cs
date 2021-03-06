﻿using System.Drawing;
using Newtonsoft.Json.Linq;

namespace Generator
{
    public class CustomJPrototypeResolver
    {
        public JObject Prototypes { get; }
        private readonly CustomJInterpreter _interpreter;

        public CustomJPrototypeResolver(JObject prototypes, CustomJInterpreter interpreter)
        {
            Prototypes = prototypes;
            _interpreter = interpreter;
        }

        public string GetStringOrDefault(JObject baseObj, string key, string defaultValue)
            => baseObj.ContainsKey("Prototype")
                ? _interpreter.GetStringOrDefault(baseObj, key, () => GetStringOrDefault((JObject)Prototypes[baseObj.GetPropertyValue("Prototype")], key, defaultValue))
                : _interpreter.GetStringOrDefault(baseObj, key, defaultValue);

        public string GetString(JObject baseObj, string key)
            => baseObj.ContainsKey("Prototype") 
                ? _interpreter.GetStringOrDefault(baseObj, key, () => GetString((JObject)Prototypes[baseObj.GetPropertyValue("Prototype")], key)) 
                : _interpreter.GetString(baseObj, key);

        public int GetIntOrDefault(JObject baseObj, string key, int defaultValue)
            => baseObj.ContainsKey("Prototype")
                ? _interpreter.GetIntOrDefault(baseObj, key, () => GetIntOrDefault((JObject)Prototypes[baseObj.GetPropertyValue("Prototype")], key, defaultValue))
                : _interpreter.GetIntOrDefault(baseObj, key, defaultValue);

        public int GetInt(JObject baseObj, string key)
            => baseObj.ContainsKey("Prototype")
                ? _interpreter.GetIntOrDefault(baseObj, key, () => GetInt((JObject)Prototypes[baseObj.GetPropertyValue("Prototype")], key))
                : _interpreter.GetInt(baseObj, key);

        public decimal GetDecimalOrDefault(JObject baseObj, string key, decimal defaultValue)
            => baseObj.ContainsKey("Prototype")
                ? _interpreter.GetDecimalOrDefault(baseObj, key, () => GetDecimalOrDefault((JObject)Prototypes[baseObj.GetPropertyValue("Prototype")], key, defaultValue))
                : _interpreter.GetDecimalOrDefault(baseObj, key, defaultValue);

        public decimal GetDecimal(JObject baseObj, string key)
            => baseObj.ContainsKey("Prototype")
                ? _interpreter.GetDecimalOrDefault(baseObj, key, () => GetDecimal((JObject)Prototypes[baseObj.GetPropertyValue("Prototype")], key))
                : _interpreter.GetDecimal(baseObj, key);

        public bool GetBoolOrDefault(JObject baseObj, string key, bool defaultValue)
            => baseObj.ContainsKey("Prototype")
                ? _interpreter.GetBoolOrDefault(baseObj, key, () => GetBoolOrDefault((JObject)Prototypes[baseObj.GetPropertyValue("Prototype")], key, defaultValue))
                : _interpreter.GetBoolOrDefault(baseObj, key, defaultValue);

        public bool GetBool(JObject baseObj, string key)
            => baseObj.ContainsKey("Prototype")
                ? _interpreter.GetBoolOrDefault(baseObj, key, () => GetBool((JObject)Prototypes[baseObj.GetPropertyValue("Prototype")], key))
                : _interpreter.GetBool(baseObj, key);

        public Color GetColorOrDefault(JObject baseObj, string key, Color defaultValue)
            => baseObj.ContainsKey("Prototype")
                ? _interpreter.GetColorOrDefault(baseObj, key, () => GetColorOrDefault((JObject)Prototypes[baseObj.GetPropertyValue("Prototype")], key, defaultValue))
                : _interpreter.GetColorOrDefault(baseObj, key, defaultValue);

        public Color GetColor(JObject baseObj, string key)
            => baseObj.ContainsKey("Prototype")
                ? _interpreter.GetColorOrDefault(baseObj, key, () => GetColor((JObject)Prototypes[baseObj.GetPropertyValue("Prototype")], key))
                : _interpreter.GetColor(baseObj, key);

        public T GetEnumOrDefault<T>(JObject baseObj, string key, T defaultValue)
            => baseObj.ContainsKey("Prototype")
                ? _interpreter.GetEnumOrDefault(baseObj, key, () => GetEnumOrDefault((JObject)Prototypes[baseObj.GetPropertyValue("Prototype")], key, defaultValue))
                : _interpreter.GetEnumOrDefault(baseObj, key, defaultValue);

        public T GetEnum<T>(JObject baseObj, string key)
            => baseObj.ContainsKey("Prototype")
                ? _interpreter.GetEnumOrDefault(baseObj, key, () => GetEnum<T>((JObject)Prototypes[baseObj.GetPropertyValue("Prototype")], key))
                : _interpreter.GetEnum<T>(baseObj, key);

        public T GetFlagsEnumOrDefault<T>(JObject baseObj, string key, T defaultValue)
            => baseObj.ContainsKey("Prototype")
                ? _interpreter.GetFlagsEnumOrDefault(baseObj, key, () => GetFlagsEnumOrDefault((JObject)Prototypes[baseObj.GetPropertyValue("Prototype")], key, defaultValue))
                : _interpreter.GetFlagsEnumOrDefault(baseObj, key, defaultValue);

        public T GetFlagsEnum<T>(JObject baseObj, string key)
            => baseObj.ContainsKey("Prototype")
                ? _interpreter.GetFlagsEnumOrDefault(baseObj, key, () => GetFlagsEnum<T>((JObject)Prototypes[baseObj.GetPropertyValue("Prototype")], key))
                : _interpreter.GetFlagsEnum<T>(baseObj, key);

        public string GetRootValue(string str) => _interpreter.GetRootValue(str);
    }
}
