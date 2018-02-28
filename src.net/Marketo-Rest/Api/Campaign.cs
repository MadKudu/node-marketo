using System.Threading.Tasks;

namespace Marketo.Api
{
    public class Campaign
    {
        readonly MarketoClient _marketo;
        readonly Connection _connection;

        public Campaign(MarketoClient marketo, Connection connection)
        {
            _marketo = marketo;
            _connection = connection;
        }

        public Task<dynamic> Request(string campaignId, string[] leads, string tokens)
        {
            var path = util.createPath("campaigns", campaignId, "trigger.json");
            dynamic options = new
            {
                input = new { leads, tokens },
            };
            options = dyn.exp(options);
            options = util.formatOptions(options);
            return _connection.postJson(path, options);
        }

        public Task<dynamic> GetCampaigns(dynamic options = null)
        {
            var path = util.createPath("campaigns.json");
            return _connection.get(path, new { data = options });
        }
    }
}
