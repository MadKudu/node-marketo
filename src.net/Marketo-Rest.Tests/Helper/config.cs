using System;

namespace Marketo.Helper
{
    public static class config
    {
        const string defaults_endpoint = "https://123-ABC-456.mktorest.com/rest";
        const string defaults_identity = "https://123-ABC-456.mktorest.com/identity";
        const string defaults_clientId = "someId";
        const string defaults_clientSecret = "someSecret";

        static config()
        {
            MarketoClient.Logger = (x) => { Console.WriteLine(x); };
        }

        public static MarketoClient Connection()
        {
            var marketo = new MarketoClient(new
            {
                endpoint = Environment.GetEnvironmentVariable("MARKETO_ENDPOINT") ?? defaults_endpoint,
                identity = Environment.GetEnvironmentVariable("MARKETO_IDENTITY") ?? defaults_identity,
                clientId = Environment.GetEnvironmentVariable("MARKETO_CLIENT_ID") ?? defaults_clientId,
                clientSecret = Environment.GetEnvironmentVariable("MARKETO_CLIENT_SECRET") ?? defaults_clientSecret,
            });
            return marketo;
        }
    }
}
