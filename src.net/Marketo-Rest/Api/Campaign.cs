using System.Threading.Tasks;

namespace Marketo.Api
{
    /// <summary>
    /// Class Campaign.
    /// </summary>
    public class Campaign
    {
        readonly MarketoClient _marketo;
        readonly Connection _connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="Campaign"/> class.
        /// </summary>
        /// <param name="marketo">The marketo.</param>
        /// <param name="connection">The connection.</param>
        public Campaign(MarketoClient marketo, Connection connection)
        {
            _marketo = marketo;
            _connection = connection;
        }

        /// <summary>
        /// Requests the specified campaign identifier.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <param name="leads">The leads.</param>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> Request(string campaignId, string[] leads, string tokens, dynamic options = null)
        {
            var path = util.createPath("campaigns", campaignId, "trigger.json");
            options = dyn.exp(options);
            options.input = new { leads, tokens };
            options = util.formatOptions(options);
            return _connection.postJson(path, options);
        }

        /// <summary>
        /// Gets the campaigns.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> GetCampaigns(dynamic options = null)
        {
            var path = util.createPath("campaigns.json");
            return _connection.get(path, new { data = options });
        }
    }
}
