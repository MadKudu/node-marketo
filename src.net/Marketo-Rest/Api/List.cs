using System.Threading.Tasks;
using System.Linq;

namespace Marketo.Api
{
    /// <summary>
    /// Class List.
    /// </summary>
    public class List
    {
        readonly MarketoClient _marketo;
        readonly Connection _connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="List"/> class.
        /// </summary>
        /// <param name="marketo">The marketo.</param>
        /// <param name="connection">The connection.</param>
        public List(MarketoClient marketo, Connection connection)
        {
            _marketo = marketo;
            _connection = connection;
        }

        /// <summary>
        /// Finds the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> Find(dynamic options = null)
        {
            options = dyn.exp(options);
            options = util.arrayToCSV(options, new[] { "id", "name", "programName", "workspaceName" });
            var path = util.createPath("lists.json");
            return _connection.get(path, new { data = options });
        }

        /// <summary>
        /// Adds the leads to list.
        /// </summary>
        /// <param name="listId">The list identifier.</param>
        /// <param name="input">The input.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> AddLeadsToList(int listId, object[] input, dynamic options = null)
        {
            var path = util.createPath("lists", listId.ToString(), "leads.json");
            input = input.Select(x =>
            {
                if (x is string || x is int) return (object)new { id = x.ToString() };
                var id = dyn.getProp<string>(x, "id") ?? dyn.getProp<int>(x, "id").ToString();
                return id != "0" ? (new { id }) : null;
            }).Where(x => x != null).ToArray();
            return _connection.postJson(path, new { input }, options);
        }

        /// <summary>
        /// Removes the leads from list.
        /// </summary>
        /// <param name="listId">The list identifier.</param>
        /// <param name="leadIds">The lead ids.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> RemoveLeadsFromList(int listId, int[] leadIds, dynamic options = null)
        {
            var path = util.createPath("lists", listId.ToString(), "leads.json");
            return _connection.del(path, new { query = new { id = string.Join("&id=", leadIds) } }, "application/json");
        }

        /// <summary>
        /// Adds the emails to list.
        /// </summary>
        /// <param name="emails">The emails.</param>
        /// <param name="listId">The list identifier.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public async Task<dynamic> AddEmailsToList(string[] emails, int listId, dynamic options = null)
        {
            var emailsAsObj = emails.Select(email => new { email }).ToArray();
            var data = await _marketo.Lead.CreateOrUpdate(emailsAsObj, new { lookupField = "email" });
            data = await AddLeadsToList(listId, (object[])data.result, options);
            return data;
        }

        /// <summary>
        /// Gets the leads.
        /// </summary>
        /// <param name="listId">The list identifier.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> GetLeads(int listId, dynamic options = null)
        {
            var path = util.createPath("list", listId.ToString(), "leads.json");
            options = dyn.exp(options);
            options.listId = listId;
            return _connection.post(path, new { data = options });
        }

        /// <summary>
        /// Determines whether the specified list identifier is member.
        /// </summary>
        /// <param name="listId">The list identifier.</param>
        /// <param name="leadIds">The lead ids.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> IsMember(int listId, int[] leadIds, dynamic options = null)
        {
            var path = util.createPath("lists", listId.ToString(), "leads", "ismember.json");
            return _connection.get(path, new { query = new { id = string.Join("&id=", leadIds) } });
        }

        /// <summary>
        /// Bies the identifier.
        /// </summary>
        /// <param name="listId">The list identifier.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> ById(int listId)
        {
            return Find(new { id = new[] { listId } });
        }
    }
}
