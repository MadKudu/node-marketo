using System.Threading.Tasks;

namespace Marketo.Api
{
    public class Activities
    {
        readonly MarketoClient _marketo;
        readonly Connection _connection;

        public Activities(MarketoClient marketo, Connection connection)
        {
            _marketo = marketo;
            _connection = connection;
        }

        public Task<dynamic> GetActivityTypes()
        {
            var path = util.createPath("activities", "types.json");
            return _connection.get(path);
        }
    }
}
