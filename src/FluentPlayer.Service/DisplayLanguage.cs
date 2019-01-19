using System.Collections.Generic;
using System.Dynamic;

namespace Magentaize.FluentPlayer.Service
{
    public class DisplayLanguage : DynamicObject
    {
        private readonly IDictionary<string, string> _properties;

        public DisplayLanguage() { }

        public DisplayLanguage(IDictionary<string, string> prop)
        {
            _properties = prop;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _properties.Keys;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_properties.ContainsKey(binder.Name))
            {
                result = _properties[binder.Name];
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (_properties.ContainsKey(binder.Name))
            {
                _properties[binder.Name] = (string) value;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}