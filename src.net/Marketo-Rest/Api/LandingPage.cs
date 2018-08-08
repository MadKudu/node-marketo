using System.Threading.Tasks;

namespace Marketo.Api
{
    /// <summary>
    /// Class LandingPage.
    /// </summary>
    public class LandingPage
    {
        readonly MarketoClient _marketo;
        readonly Connection _connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="LandingPage"/> class.
        /// </summary>
        /// <param name="marketo">The marketo.</param>
        /// <param name="connection">The connection.</param>
        public LandingPage(MarketoClient marketo, Connection connection)
        {
            _marketo = marketo;
            _connection = connection;
        }

        /// <summary>
        /// Gets the landing pages.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> GetLandingPages(dynamic options = null)
        {
            var path = util.createAssetPath("landingPages.json");
            return _connection.get(path, new { data = options });
        }
    }
}
