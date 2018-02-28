using System.Threading.Tasks;

namespace Marketo.Api
{
    public class LandingPage
    {
        readonly MarketoClient _marketo;
        readonly Connection _connection;

        public LandingPage(MarketoClient marketo, Connection connection)
        {
            _marketo = marketo;
            _connection = connection;
        }

        public Task<dynamic> GetLandingPages(dynamic options = null)
        {
            var path = util.createAssetPath("landingPages.json");
            return _connection.get(path, new { data = options });
        }
    }
}
