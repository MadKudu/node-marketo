using System.Threading.Tasks;

namespace Marketo.Api
{
    public class Stats
    {
        readonly MarketoClient _marketo;
        readonly Connection _connection;

        public Stats(MarketoClient marketo, Connection connection)
        {
            _marketo = marketo;
            _connection = connection;
        }

        public Task<dynamic> Usage()
        {
            var path = util.createPath("stats", "usage.json");
            return _connection.get(path);
        }

        public Task<dynamic> UsageLast7Days()
        {
            var path = util.createPath("stats", "usage", "last7days.json");
            return _connection.get(path);
        }

        public Task<dynamic> Errors()
        {
            var path = util.createPath("stats", "errors.json");
            return _connection.get(path);
        }

        public Task<dynamic> ErrorsLast7Days()
        {
            var path = util.createPath("stats", "errors", "last7days.json");
            return _connection.get(path);
        }
    }
}
