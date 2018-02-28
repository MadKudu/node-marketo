using Marketo.Require;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Marketo
{
    internal static class util
    {
        static Action<string> _logger = (x) => { };

        public static string createAssetPath(params string[] args)
        {
            var path = $"/asset{config.api_version}/{string.Join("/", args)}";
            return path;
        }

        public static string createPath(params string[] args)
        {
            var path = $"{config.api_version}/{string.Join("/", args)}";
            return path;
        }

        public static string createBulkPath(params string[] args)
        {
            var path = $"/../bulk{config.api_version}/{string.Join("/", args)}";
            return path;
        }

        public static Action<string> logger
        {
            get { return _logger; }
            set { _logger = value ?? throw new ArgumentNullException("value"); }
        }

        public static dynamic arrayToCSV(dynamic options, string[] keys)
        {
            var copy = (IDictionary<string, object>)dyn.clone(options);
            foreach (var key in keys)
            {
                if (!copy.TryGetValue(key, out object value))
                    continue;
                if (value is string[]) copy[key] = string.Join(",", (string[])value);
                else if (value is object[]) copy[key] = string.Join(",", ((object[])value));
                //else if (value is object[]) copy[key] = string.Join(",", ((object[])value).Select(x => x.ToString()).ToArray());
            }
            return copy;
        }

        public static dynamic formatOptions(dynamic options, params string[] keys)
        {
            if (keys != null && keys.Length > 0)
                options = dyn.pick(options, keys);
            return arrayToCSV(options, new[] { "fields", "filterValues" });
        }

    }
}
