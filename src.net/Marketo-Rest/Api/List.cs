using System.Threading.Tasks;
using System.Linq;

namespace Marketo.Api
{
    public class List
    {
        readonly MarketoClient _marketo;
        readonly Connection _connection;

        public List(MarketoClient marketo, Connection connection)
        {
            _marketo = marketo;
            _connection = connection;
        }

        public Task<dynamic> Find(dynamic options = null)
        {
            options = dyn.exp(options);
            options = util.arrayToCSV(options, new[] { "id", "name", "programName", "workspaceName" });
            var path = util.createPath("lists.json");
            return _connection.get(path, new { data = options });
        }

        public Task<dynamic> AddLeadsToList(int listId, object[] input)
        {
            var path = util.createPath("lists", listId.ToString(), "leads.json");
            input = input.Select(x =>
            {
                if (x is string || x is int) return (object)new { id = x.ToString() };
                var id = dyn.getProp<string>(x, "id") ?? dyn.getProp<int>(x, "id").ToString();
                return id != "0" ? (new { id }) : null;
            }).Where(x => x != null).ToArray();
            return _connection.postJson(path, new { input });
        }

        public Task<dynamic> RemoveLeadsFromList(int listId, int[] leadIds)
        {
            var path = util.createPath("lists", listId.ToString(), "leads.json");
            return _connection.del(path, new { query = new { id = string.Join("&id=", leadIds) } }, "application/json");
        }

        public async Task<dynamic> AddEmailsToList(string[] emails, int listId, dynamic options = null)
        {
            var emailsAsObj = emails.Select(email => new { email }).ToArray();
            var data = await _marketo.Lead.CreateOrUpdate(emailsAsObj, new { lookupField = "email" });
            data = await AddLeadsToList(listId, (object[])data.result);
            return data;
        }

        public Task<dynamic> GetLeads(int listId, dynamic options = null)
        {
            var path = util.createPath("list", listId.ToString(), "leads.json");
            options = dyn.exp(options);
            options.listId = listId;
            return _connection.post(path, new { data = options });
        }

        public Task<dynamic> IsMember(int listId, int[] leadIds)
        {
            var path = util.createPath("lists", listId.ToString(), "leads", "ismember.json");
            return _connection.get(path, new { query = new { id = string.Join("&id=", leadIds) } });
        }

        public Task<dynamic> ById(int listId)
        {
            return Find(new { id = new[] { listId } });
        }
    }
}
