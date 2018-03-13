using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Marketo
{
    /// <summary>
    /// Class dyn.
    /// </summary>
    public static class dyn
    {
        /// <summary>
        /// Determines whether the specified s has property.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if the specified s has property; otherwise, <c>false</c>.</returns>
        public static bool hasProp(object s, string name)
        {
            if (s == null) return false;
            if (s is ExpandoObject) return ((IDictionary<string, object>)s).ContainsKey(name);
            if (s is ExpandedObject) return ((ExpandedObject)s).TryGetMember(name, out object result);
            return s.GetType().GetProperty(name) != null;
        }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s">The s.</param>
        /// <param name="name">The name.</param>
        /// <param name="default_">The default.</param>
        /// <returns>T.</returns>
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

        /// <summary>
        /// Clones the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>System.Object.</returns>
        public static object clone(object options)
        {
            return getData(options).ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Picks the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="keys">The keys.</param>
        /// <returns>System.Object.</returns>
        public static object pick(object options, string[] keys)
        {
            return keys.ToDictionary(x => x, x => getProp<object>(options, x));
        }

        /// <summary>
        /// To the j object.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns>JObject.</returns>
        public static JObject ToJObject(object s)
        {
            return JObject.Parse(JsonConvert.SerializeObject(s));
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns>IEnumerable&lt;KeyValuePair&lt;System.String, System.Object&gt;&gt;.</returns>
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

        /// <summary>
        /// Gets the data as string.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns>IEnumerable&lt;KeyValuePair&lt;System.String, System.String&gt;&gt;.</returns>
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

        /// <summary>
        /// Gets the object.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="name">The name.</param>
        /// <param name="default_">The default.</param>
        /// <param name="emptyIfNull">if set to <c>true</c> [empty if null].</param>
        /// <returns>System.Object.</returns>
        public static object getObj(object s, string name, object default_ = null, bool emptyIfNull = true)
        {
            var r = getProp(s, name, default_);
            return exp(r, emptyIfNull);
        }

        /// <summary>
        /// Exps the specified s.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="emptyIfNull">if set to <c>true</c> [empty if null].</param>
        /// <returns>System.Object.</returns>
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
