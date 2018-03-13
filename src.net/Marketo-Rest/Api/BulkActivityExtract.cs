using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Marketo.Api
{
    public class BulkActivityExtract
    {
        readonly Action<string> _log = util.logger;
        readonly MarketoClient _marketo;
        readonly Connection _connection;
        readonly Retry _retry;

        public BulkActivityExtract(MarketoClient marketo, Connection connection)
        {
            _marketo = marketo;
            _connection = connection;
            _retry = new Retry(new { maxRetries = 10, initialDelay = 30000, maxDelay = 60000 });
        }

        public Task<dynamic> Create(dynamic filter, dynamic options = null)
        {
            var path = util.createBulkPath("activities", "export", "create.json");
            options = dyn.exp(options);
            options.filter = filter;
            return _connection.postJson(path, options);
        }

        public Task<dynamic> Enqueue(string exportId, dynamic options = null)
        {
            var path = util.createPath("activities", "export", exportId, "enqueue.json");
            return _connection.post(path, new { data = options });
        }

        public Task<dynamic> Status(string exportId, dynamic options = null)
        {
            var path = util.createPath("activities", "export", exportId, "status.json");
            return _connection.get(path, new { data = options });
        }

        public async Task<dynamic> StatusTilCompleted(string exportId, dynamic options = null)
        {
            Func<bool, Task<JObject>> requestFn = async (forceOAuth) =>
            {
                dynamic data = await Status(exportId);
                if (!(bool)data.success)
                {
                    var msg = (string)data.errors[0].message;
                    _log(msg);
                    throw new MarketoException(msg);
                }
                Console.WriteLine($"STATUS: {data.result[0].status}");
                if (data.result[0].status == "Queued" || data.result[0].status == "Processing")
                    throw new MarketoException(null)
                    {
                        Id = data["requestId"],
                        Code = 606
                    };
                return data;
            };
            return await _retry.start(requestFn);
        }

        public Task<dynamic> Cancel(string exportId, dynamic options = null)
        {
            var path = util.createPath("activities", "export", exportId, "cancel.json");
            return _connection.post(path, new { data = options });
        }

        public async Task<dynamic> Get(dynamic filter, dynamic options = null)
        {
            var data = await Create(filter, options);
            if (!(bool)data.success)
            {
                var msg = (string)data.errors[0].message;
                _log(msg);
                throw new MarketoException(msg);
            }
            var exportId = (string)data.results[0].exportId;
            try
            {
                data = await Enqueue(exportId);
                if (!(bool)data.success)
                {
                    var msg = (string)data.errors[0].message;
                    _log(msg);
                    throw new MarketoException(msg);
                }
                data = await StatusTilCompleted(exportId);
            }
            catch (Exception err) { await Cancel(exportId); throw err; }
            return data;
        }

        public Task<dynamic> File(string exportId, dynamic options = null)
        {
            var path = util.createPath("activities", "export", exportId, "file.json");
            return _connection.get(path, new { data = options });
        }
    }
}
