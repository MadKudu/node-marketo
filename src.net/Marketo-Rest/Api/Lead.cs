using System;
using System.Threading.Tasks;

namespace Marketo.Api
{
    /// <summary>
    /// Class Lead.
    /// </summary>
    public class Lead
    {
        readonly Action<string> _log = util.logger;
        readonly MarketoClient _marketo;
        readonly Connection _connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="Lead"/> class.
        /// </summary>
        /// <param name="marketo">The marketo.</param>
        /// <param name="connection">The connection.</param>
        public Lead(MarketoClient marketo, Connection connection)
        {
            _marketo = marketo;
            _connection = connection;
        }

        /// <summary>
        /// Bies the identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> ById(int id, dynamic options = null)
        {
            var path = util.createPath("lead", $"{id}.json");
            options = dyn.exp(options);
            options = util.formatOptions(options, "fields");
            return _connection.get(path, new { query = options });
        }

        /// <summary>
        /// Finds the specified filter type.
        /// </summary>
        /// <param name="filterType">Type of the filter.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> Find(string filterType, object[] filterValues, dynamic options = null)
        {
            var path = util.createPath("leads.json");
            options = dyn.exp(options);
            options.filterType = filterType;
            options.filterValues = filterValues;
            options = util.formatOptions(options);
            return _connection.get(path, new { query = options });
        }

        /// <summary>
        /// Creates the or update.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
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

        /// <summary>
        /// Pushes the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
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

        /// <summary>
        /// Associates the lead.
        /// </summary>
        /// <param name="leadId">The lead identifier.</param>
        /// <param name="cookieId">The cookie identifier.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> AssociateLead(int leadId, string cookieId, dynamic options = null)
        {
            var path = util.createPath("leads", leadId.ToString(), "associate.json");
            return _connection.postJson(path, new { }, new { query = new { cookie = cookieId } });
        }

        /// <summary>
        /// Describes the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> Describe(dynamic options = null)
        {
            var path = util.createPath("leads", "describe.json");
            return _connection.get(path, options);
        }

        /// <summary>
        /// Partitionses the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> Partitions(dynamic options = null)
        {
            var path = util.createPath("leads", "partitions.json");
            return _connection.get(path, options);
        }

        /// <summary>
        /// Merges the lead.
        /// </summary>
        /// <param name="winningLead">The winning lead.</param>
        /// <param name="losingLead">The losing lead.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> MergeLead(int winningLead, int[] losingLead, dynamic options = null)
        {
            var path = util.createPath("leads", winningLead.ToString(), "merge.json");
            return _connection.postJson(path, new { data = options }, new { query = new { leadIds = string.Join(",", losingLead) } });
        }

        /// <summary>
        /// Gets the page token.
        /// </summary>
        /// <param name="sinceDatetime">The since datetime.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> GetPageToken(DateTime sinceDatetime, dynamic options = null)
        {
            var path = util.createPath("activities", "pagingtoken.json");
            options = dyn.exp(options);
            options.sinceDatetime = sinceDatetime;
            options = util.formatOptions(options);
            return _connection.post(path, new { data = options });
        }

        /// <summary>
        /// Gets the activities.
        /// </summary>
        /// <param name="leadId">The lead identifier.</param>
        /// <param name="activityId">The activity identifier.</param>
        /// <param name="pageToken">The page token.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> GetActivities(int[] leadId, int[] activityId, string pageToken, dynamic options = null)
        {
            options = dyn.exp(options);
            options.leadIds = string.Join(",", leadId);
            options.activityTypeIds = string.Join(",", activityId);
            options.nextPageToken = pageToken;
            return GetActivitiesWithOptions(options);
        }

        /// <summary>
        /// Gets the activities for list.
        /// </summary>
        /// <param name="listId">The list identifier.</param>
        /// <param name="activityId">The activity identifier.</param>
        /// <param name="pageToken">The page token.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> GetActivitiesForList(int listId, int[] activityId, string pageToken, dynamic options = null)
        {
            options = dyn.exp(options);
            options.listId = listId;
            options.activityTypeIds = string.Join(",", activityId);
            options.nextPageToken = pageToken;
            return GetActivitiesWithOptions(options);
        }

        /// <summary>
        /// Gets the activities with options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> GetActivitiesWithOptions(dynamic options = null)
        {
            var path = util.createPath("activities.json");
            options = util.formatOptions(options);
            return _connection.post(path, new { data = options });
        }

        /// <summary>
        /// Gets the changes.
        /// </summary>
        /// <param name="pageToken">The page token.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> GetChanges(string pageToken, dynamic options = null)
        {
            options = dyn.exp(options);
            options.nextPageToken = pageToken;
            return GetChangesWithOptions(options);
        }

        /// <summary>
        /// Gets the changes with options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> GetChangesWithOptions(dynamic options = null)
        {
            var path = util.createPath("activities", "leadchanges.json");
            options = util.formatOptions(options);
            return _connection.post(path, new { data = options });
        }
    }
}
