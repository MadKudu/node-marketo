using System.Threading.Tasks;

namespace Marketo.Api
{
    /// <summary>
    /// Class Stats.
    /// </summary>
    public class Stats
    {
        readonly MarketoClient _marketo;
        readonly Connection _connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="Stats"/> class.
        /// </summary>
        /// <param name="marketo">The marketo.</param>
        /// <param name="connection">The connection.</param>
        public Stats(MarketoClient marketo, Connection connection)
        {
            _marketo = marketo;
            _connection = connection;
        }

        /// <summary>
        /// Usages this instance.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> Usage(dynamic options = null)
        {
            var path = util.createPath("stats", "usage.json");
            return _connection.get(path, options);
        }

        /// <summary>
        /// Usages the last7 days.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> UsageLast7Days(dynamic options = null)
        {
            var path = util.createPath("stats", "usage", "last7days.json");
            return _connection.get(path, options);
        }

        /// <summary>
        /// Errorses this instance.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> Errors(dynamic options = null)
        {
            var path = util.createPath("stats", "errors.json");
            return _connection.get(path, options);
        }

        /// <summary>
        /// Errorses the last7 days.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> ErrorsLast7Days(dynamic options = null)
        {
            var path = util.createPath("stats", "errors", "last7days.json");
            return _connection.get(path, options);
        }
    }
}
