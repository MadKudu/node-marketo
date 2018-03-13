using Marketo.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Marketo
{
    /// <summary>
    /// Class MarketoClient.
    /// </summary>
    public class MarketoClient
    {
        Connection _connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketoClient"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public MarketoClient(dynamic options)
        {
            _connection = new Connection(options);
            //
            Campaign = new Campaign(this, _connection);
            Email = new Email(this, _connection);
            LandingPage = new LandingPage(this, _connection);
            List = new List(this, _connection);
            Lead = new Lead(this, _connection);
            Stats = new Stats(this, _connection);
            Activities = new Activities(this, _connection);
        }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <value>The connection.</value>
        public Connection Connection => _connection;

        /// <summary>
        /// Gets the campaign.
        /// </summary>
        /// <value>The campaign.</value>
        public Campaign Campaign { get; private set; }
        /// <summary>
        /// Gets the email.
        /// </summary>
        /// <value>The email.</value>
        public Email Email { get; private set; }
        /// <summary>
        /// Gets the landing page.
        /// </summary>
        /// <value>The landing page.</value>
        public LandingPage LandingPage { get; private set; }
        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <value>The list.</value>
        public List List { get; private set; }
        /// <summary>
        /// Gets the lead.
        /// </summary>
        /// <value>The lead.</value>
        public Lead Lead { get; private set; }
        /// <summary>
        /// Gets the stats.
        /// </summary>
        /// <value>The stats.</value>
        public Stats Stats { get; private set; }
        /// <summary>
        /// Gets the activities.
        /// </summary>
        /// <value>The activities.</value>
        public Activities Activities { get; private set; }

        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        /// <value>The access token.</value>
        public string AccessToken
        {
            get => _connection.AccessToken;
            set => _connection.AccessToken = value;
        }

        /// <summary>
        /// Gets or sets the on access token.
        /// </summary>
        /// <value>The on access token.</value>
        public Action<Connection> OnAccessToken
        {
            get => _connection.OnAccessToken;
            set => _connection.OnAccessToken = value;
        }

        /// <summary>
        /// Gets or sets the on response.
        /// </summary>
        /// <value>The on response.</value>
        public Action<Connection> OnResponse
        {
            get => _connection.OnResponse;
            set => _connection.OnResponse = value;
        }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>The logger.</value>
        public static Action<string> Logger
        {
            get => util.logger;
            set => util.logger = value;
        }

        /// <summary>
        /// The json serializer settings
        /// </summary>
        internal static JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
        };
    }
}
