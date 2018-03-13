using System.Threading.Tasks;

namespace Marketo.Api
{
    /// <summary>
    /// Class Email.
    /// </summary>
    public class Email
    {
        readonly MarketoClient _marketo;
        readonly Connection _connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="Email"/> class.
        /// </summary>
        /// <param name="marketo">The marketo.</param>
        /// <param name="connection">The connection.</param>
        public Email(MarketoClient marketo, Connection connection)
        {
            _marketo = marketo;
            _connection = connection;
        }

        /// <summary>
        /// Updates the email content editable text.
        /// </summary>
        /// <param name="emailId">The email identifier.</param>
        /// <param name="htmlId">The HTML identifier.</param>
        /// <param name="html">The HTML.</param>
        /// <param name="text">The text.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> UpdateEmailContentEditableText(string emailId, string htmlId, string html, string text, dynamic options = null)
        {
            var path = util.createAssetPath("email", emailId, "content", $"{htmlId}.json");
            return _connection.post(path, new { data = new { type = "Text", value = html, textValue = text } });
        }

        /// <summary>
        /// Approves the email.
        /// </summary>
        /// <param name="emailId">The email identifier.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> ApproveEmail(string emailId, dynamic options = null)
        {
            var path = util.createAssetPath("email", emailId, "approveDraft.json");
            return _connection.post(path, new { data = options });
        }

        /// <summary>
        /// Gets the content of the email.
        /// </summary>
        /// <param name="emailId">The email identifier.</param>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> GetEmailContent(string emailId, dynamic options = null)
        {
            var path = util.createAssetPath("email", emailId, "content.json");
            return _connection.get(path, new { data = options });
        }

        /// <summary>
        /// Gets the emails.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>Task&lt;dynamic&gt;.</returns>
        public Task<dynamic> GetEmails(dynamic options = null)
        {
            var path = util.createAssetPath("emails.json");
            return _connection.get(path, new { data = options });
        }
    }
}
