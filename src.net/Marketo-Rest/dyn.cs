using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Marketo
{
    public static class dyn
    {
        public static bool hasProp(object s, string name)
        {
            if (s == null) return false;
            if (s is ExpandoObject) return ((IDictionary<string, object>)s).ContainsKey(name);
            if (s is ExpandedObject) return ((ExpandedObject)s).TryGetMember(name, out object result);
            return s.GetType().GetProperty(name) != null;
        }

        public static T getProp<T>(object s, string name, T default_ = default(T))
        {
            if (s == null) return default_;
            if (s is ExpandoObject)
            {
                var dyno = (IDictionary<string, object>)s;
                return dyno.ContainsKey(name) ? (T)dyno[name] : default_;
            }
            else if (s is ExpandedObject)
            {
                var dyno = (ExpandedObject)s;
                return dyno.TryGetMember(name, out object result) ? (T)result : default_;
            }
            var prop = s.GetType().GetProperty(name);
            return prop != null ? (T)prop.GetValue(s) : default_;
        }

        public static object clone(object options)
        {
            return getData(options).ToDictionary(x => x.Key, x => x.Value);
        }

        public static object pick(object options, string[] keys)
        {
            return keys.ToDictionary(x => x, x => getProp<object>(options, x));
        }

        public static JObject ToJObject(object s)
        {
            return JObject.Parse(JsonConvert.SerializeObject(s));
        }

        public static IEnumerable<KeyValuePair<string, object>> getData(object s)
        {
            if (s is ExpandoObject)
            {
                var dyno = (IDictionary<string, object>)s;
                return dyno.Select(x => new KeyValuePair<string, object>(x.Key, x.Value)).ToArray();
            }
            else if (s is ExpandedObject)
            {
                var dyno = (ExpandedObject)s;
                return dyno.GetData();
            }
            return s.GetType().GetProperties()
                .Where(x => x.CanRead && x.GetValue(s, null) != null)
                .Select(x => new KeyValuePair<string, object>(x.Name, x.GetValue(s, null)))
                .ToList();
        }

        public static IEnumerable<KeyValuePair<string, string>> getDataAsString(object s)
        {
            if (s is ExpandoObject)
            {
                var dyno = (IDictionary<string, object>)s;
                return dyno.Select(x => new KeyValuePair<string, string>(x.Key, x.Value.ToString())).ToArray();
            }
            else if (s is ExpandedObject)
            {
                var dyno = (ExpandedObject)s;
                return dyno.GetDataAsString();
            }
            return s.GetType().GetProperties()
                .Where(x => x.CanRead && x.GetValue(s, null) != null)
                .Select(x => new KeyValuePair<string, string>(x.Name, x.GetValue(s, null).ToString()))
                .ToList();
        }

        public static object getObj(object s, string name, object default_ = null, bool emptyIfNull = true)
        {
            var r = getProp(s, name, default_);
            return exp(r, emptyIfNull);
        }

        public static object exp(object s, bool emptyIfNull = false)
        {
            if (s == null) return emptyIfNull ? new ExpandedObject(null) : null;
            return s is ExpandedObject ? s : new ExpandedObject(s);
        }

        internal class ExpandedObject : DynamicObject
        {
            readonly Dictionary<string, object> _customProperties = new Dictionary<string, object>();
            readonly object _object;

            public ExpandedObject(object sealedObject)
            {
                _object = sealedObject;
            }

            private PropertyInfo GetPropertyInfo(string propertyName)
            {
                return _object != null ? _object.GetType().GetProperties().FirstOrDefault(propertyInfo => propertyInfo.Name == propertyName) : null;
            }

            public IEnumerable<KeyValuePair<string, object>> GetData()
            {
                var list = _customProperties.Select(x => new KeyValuePair<string, object>(x.Key, x.Value)).ToList();
                if (_object != null) list.AddRange(_object.GetType().GetProperties().Select(x => new KeyValuePair<string, object>(x.Name, x.GetValue(_object))));
                return list;
            }

            public IEnumerable<KeyValuePair<string, string>> GetDataAsString()
            {
                var list = _customProperties.Select(x => new KeyValuePair<string, string>(x.Key, x.Value.ToString())).ToList();
                if (_object != null) list.AddRange(_object.GetType().GetProperties().Select(x => new KeyValuePair<string, string>(x.Name, x.GetValue(_object).ToString())));
                return list;
            }

            public bool TryGetMember(string name, out object result)
            {
                var prop = GetPropertyInfo(name);
                if (prop != null) { result = prop.GetValue(_object); return true; }
                return _customProperties.TryGetValue(name, out result);
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                var prop = GetPropertyInfo(binder.Name);
                if (prop != null) { result = prop.GetValue(_object); return true; }
                _customProperties.TryGetValue(binder.Name, out result);
                return true;
            }

            public bool TrySetMember(string name, object value)
            {
                var prop = GetPropertyInfo(name);
                if (prop != null) { prop.SetValue(_object, value); return true; }
                if (_customProperties.ContainsKey(name)) _customProperties[name] = value;
                else _customProperties.Add(name, value);
                return true;
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                var prop = GetPropertyInfo(binder.Name);
                if (prop != null) { prop.SetValue(_object, value); return true; }
                if (_customProperties.ContainsKey(binder.Name)) _customProperties[binder.Name] = value;
                else _customProperties.Add(binder.Name, value);
                return true;
            }
        }
    }
}
