using GoogleSheetsToJsonConvertor.Extensions;
using Newtonsoft.Json.Linq;

namespace GoogleSheetsToJsonConvertor.Modifications
{
    public class AddModification
    {
        private readonly int _position;
        private readonly string _addition;
        private readonly string _property;

        public AddModification(JObject add, string property)
        {
            _position = int.Parse(add.GetPropertyValue("Position"));
            _addition = add.GetPropertyValue("Addition");
            _property = property;
        }

        public string Get()
        {
            return _property.Substring(0, _position) + _addition + _property.Substring(_position);
        }
    }
}
