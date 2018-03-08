using Marketo.Api;
using System;

namespace Marketo
{
    public class MarketoClient
    {
        Connection _connection;

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

        public Campaign Campaign { get; private set; }
        public Email Email { get; private set; }
        public LandingPage LandingPage { get; private set; }
        public List List { get; private set; }
        public Lead Lead { get; private set; }
        public Stats Stats { get; private set; }
        public Activities Activities { get; private set; }

        public string AccessToken
        {
            get => _connection.AccessToken;
            set => _connection.AccessToken = value;
        }

        public Action<Connection> OnAccessToken
        {
            get => _connection.OnAccessToken;
            set => _connection.OnAccessToken = value;
        }

        public Action<Connection> OnResponse
        {
            get => _connection.OnResponse;
            set => _connection.OnResponse = value;
        }

        public static Action<string> Logger
        {
            get => util.logger;
            set => util.logger = value;
        }
    }
}
