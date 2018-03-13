using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Marketo.Api
{
    /// <summary>
    /// Class BulkLeadExtract.
    /// </summary>
    public class BulkLeadExtract
    {
        readonly Action<string> _log = util.logger;
        readonly MarketoClient _marketo;
        readonly Connection _connection;
        readonly Retry _retry;

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkLeadExtract"/> class.
        /// </summary>
        /// <param name="marketo">The marketo.</param>
        /// <param name="connection">The connection.</param>
        public BulkLeadExtract(MarketoClient marketo, Connection connection)
        {
            _marketo = marketo;
            _connection = connection;
            _retry = new Retry(new { maxRetries = 10, initialDelay = 30000, maxDelay = 60000 });
        }

        /// <summary>
        /// Creates the specified fields.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> Create(string[] fields, dynamic filter, dynamic options = null)
        {
            var path = util.createBulkPath("leads", "export", "create.json");
            options = dyn.exp(options);
            options.fields = fields;
            options.filter = filter;
            return _connection.postJson(path, options);
        }

        /// <summary>
        /// Enqueues the specified export identifier.
        /// </summary>
        /// <param name="exportId">The export identifier.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> Enqueue(string exportId, dynamic options = null)
        {
            var path = util.createPath("leads", "export", exportId, "enqueue.json");
            return _connection.post(path, new { data = options });
        }

        /// <summary>
        /// Statuses the specified export identifier.
        /// </summary>
        /// <param name="exportId">The export identifier.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> Status(string exportId, dynamic options = null)
        {
            var path = util.createPath("leads", "export", exportId, "status.json");
            return _connection.get(path, new { data = options });
        }

        /// <summary>
        /// Statuses the til completed.
        /// </summary>
        /// <param name="exportId">The export identifier.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
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

        /// <summary>
        /// Cancels the specified export identifier.
        /// </summary>
        /// <param name="exportId">The export identifier.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> Cancel(string exportId, dynamic options = null)
        {
            var path = util.createPath("leads", "export", exportId, "cancel.json");
            return _connection.post(path, new { data = options });
        }

        /// <summary>
        /// Gets the specified filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        /// <exception cref="MarketoException">
        /// </exception>
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

        /// <summary>
        /// Files the specified export identifier.
        /// </summary>
        /// <param name="exportId">The export identifier.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> File(string exportId, dynamic options = null)
        {
            var path = util.createPath("leads", "export", exportId, "file.json");
            return _connection.get(path, new { data = options });
        }
    }
}
