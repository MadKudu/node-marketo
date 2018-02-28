using System.Threading.Tasks;

namespace Marketo.Api
{
    public class Email
    {
        readonly MarketoClient _marketo;
        readonly Connection _connection;

        public Email(MarketoClient marketo, Connection connection)
        {
            _marketo = marketo;
            _connection = connection;
        }

        public Task<dynamic> UpdateEmailContentEditableText(string emailId, string htmlId, string html, string text, dynamic options = null)
        {
            var path = util.createAssetPath("email", emailId, "content", $"{htmlId}.json");
            return _connection.post(path, new { data = new { type = "Text", value = html, textValue = text } });
        }

        public Task<dynamic> ApproveEmail(string emailId, dynamic options = null)
        {
            var path = util.createAssetPath("email", emailId, "approveDraft.json");
            return _connection.post(path, new { data = options });
        }

        public Task<dynamic> GetEmailContent(string emailId, dynamic options = null)
        {
            var path = util.createAssetPath("email", emailId, "content.json");
            return _connection.get(path, new { data = options });
        }

        public Task<dynamic> GetEmails(dynamic options = null)
        {
            var path = util.createAssetPath("emails.json");
            return _connection.get(path, new { data = options });
        }
    }
}
