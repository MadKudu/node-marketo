using System;
using System.Threading.Tasks;

namespace Marketo.Api
{
    public class Lead
    {
        readonly Action<string> _log = util.logger;
        readonly MarketoClient _marketo;
        readonly Connection _connection;

        public Lead(MarketoClient marketo, Connection connection)
        {
            _marketo = marketo;
            _connection = connection;
        }

        public Task<dynamic> ById(int id, dynamic options = null)
        {
            var path = util.createPath("lead", $"{id}.json");
            options = dyn.exp(options);
            options = util.formatOptions(options, "fields");
            return _connection.get(path, new { query = options });
        }

        public Task<dynamic> Find(string filterType, object[] filterValues, dynamic options = null)
        {
            var path = util.createPath("leads.json");
            options = dyn.exp(options);
            options.filterType = filterType;
            options.filterValues = filterValues;
            options = util.formatOptions(options);
            return _connection.get(path, new { query = options });
        }

        public Task<dynamic> CreateOrUpdate(object[] input, dynamic options = null)
        {
            var path = util.createPath("leads.json");
            options = dyn.exp(options);
            options.input = input;
            options = util.formatOptions(options, "fields");
            return _connection.postJson(path, options);
            //Task<dynamic> then(dynamic data)
            //{
            //    if (data.success == true)
            //        return data;
            //    _log($"Cannot create/update lead: {data}");
            //    throw new MarketoException("Cannot get lead(s) from input: {input}");
            //}
        }

        public Task<dynamic> Push(object[] input, dynamic options = null)
        {
            var path = util.createPath("leads", "push.json");
            options = dyn.exp(options);
            options.input = input;
            return _connection.postJson(path, options);
            //Task<dynamic> then(dynamic data)
            //{
            //    if (data.success == true)
            //        return data;
            //    _log($"Cannot push lead: {data}");
            //    throw new MarketoException("Cannot get lead(s) from input: {input}");
            //}
        }

        public Task<dynamic> AssociateLead(int leadId, string cookieId)
        {
            var path = util.createPath("leads", leadId.ToString(), "associate.json");
            return _connection.postJson(path, new { }, new { query = new { cookie = cookieId } });
        }

        public Task<dynamic> Describe()
        {
            var path = util.createPath("leads", "describe.json");
            return _connection.get(path);
        }

        public Task<dynamic> Partitions()
        {
            var path = util.createPath("leads", "partitions.json");
            return _connection.get(path);
        }

        public Task<dynamic> MergeLead(int winningLead, int[] losingLead, dynamic options = null)
        {
            var path = util.createPath("leads", winningLead.ToString(), "merge.json");
            return _connection.postJson(path, new { data = options }, new { query = new { leadIds = string.Join(",", losingLead) } });
        }

        public Task<dynamic> GetPageToken(DateTime sinceDatetime, dynamic options = null)
        {
            var path = util.createPath("activities", "pagingtoken.json");
            options = dyn.exp(options);
            options.sinceDatetime = sinceDatetime;
            options = util.formatOptions(options);
            return _connection.post(path, new { data = options });
        }

        public Task<dynamic> GetActivities(int[] leadId, int[] activityId, string pageToken, dynamic options = null)
        {
            options = dyn.exp(options);
            options.leadIds = string.Join(",", leadId);
            options.activityTypeIds = string.Join(",", activityId);
            options.nextPageToken = pageToken;
            return GetActivitiesWithOptions(options);
        }

        public Task<dynamic> GetActivitiesForList(int listId, int[] activityId, string pageToken, dynamic options = null)
        {
            options = dyn.exp(options);
            options.listId = listId;
            options.activityTypeIds = string.Join(",", activityId);
            options.nextPageToken = pageToken;
            return GetActivitiesWithOptions(options);
        }

        public Task<dynamic> GetActivitiesWithOptions(dynamic options = null)
        {
            var path = util.createPath("activities.json");
            options = util.formatOptions(options);
            return _connection.post(path, new { data = options });
        }

        public Task<dynamic> GetChanges(string pageToken, dynamic options = null)
        {
            options = dyn.exp(options);
            options.nextPageToken = pageToken;
            return GetChangesWithOptions(options);
        }

        public Task<dynamic> GetChangesWithOptions(dynamic options = null)
        {
            var path = util.createPath("activities", "leadchanges.json");
            options = util.formatOptions(options);
            return _connection.post(path, new { data = options });
        }
    }
}
