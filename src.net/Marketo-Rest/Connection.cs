using Marketo.Require;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Marketo
{
    public class Connection
    {
        internal static readonly Regex HttpTestExp = new Regex(@"^https?://", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        readonly ApiUsage _lastApiUsage = new ApiUsage();
        readonly Action<string> _log = util.logger;
        readonly Restler _rest = new Restler();
        readonly dynamic _options;
        readonly Retry _retry;
        JObject _tokenData;

        public class ApiUsage
        {
            public string Quota { get; set; }
            public int? QuotaUsed { get; set; }
        }

        public ApiUsage LastApiUsage => _lastApiUsage;

        public Connection(dynamic options)
        {
            _options = options ?? new { };
            _tokenData = dyn.hasProp(_options, "accessToken") ? dyn.ToJObject(new { access_token = dyn.getProp<string>(_options, "accessToken") }) : null;
            _retry = new Retry(dyn.getProp(_options, "retry", new { }));
        }

        public string AccessToken
        {
            get { return _tokenData != null ? (string)_tokenData["access_token"] : null; }
            set { _tokenData = dyn.hasProp(_options, "accessToken") ? dyn.ToJObject(new { access_token = value }) : null; }
        }

        public Action<Connection> OnAccessToken { get; set; }
        public Action<Connection> OnResponse { get; set; }

        public Task<dynamic> get(string url, dynamic options = null, string contentType = null) => _request(url, null, options, Restler.Method.GET, contentType);
        public Task<dynamic> post(string url, dynamic options = null, string contentType = null) => _request(url, null, options, Restler.Method.POST, contentType);
        public Task<dynamic> put(string url, dynamic options = null, string contentType = null) => _request(url, null, options, Restler.Method.PUT, contentType);
        public Task<dynamic> del(string url, dynamic options = null, string contentType = null) => _request(url, null, options, Restler.Method.DELETE, contentType);
        public Task<dynamic> head(string url, dynamic options = null, string contentType = null) => _request(url, null, options, Restler.Method.HEAD, contentType);
        public Task<dynamic> patch(string url, dynamic options = null, string contentType = null) => _request(url, null, options, Restler.Method.PATCH, contentType);
        public Task<dynamic> json(string url, object data, dynamic options = null) => _request(url, data, options, Restler.Method.GET);
        public Task<dynamic> postJson(string url, object data, dynamic options = null) => _request(url, data, options, Restler.Method.POST);
        public Task<dynamic> putJson(string url, object data, dynamic options = null) => _request(url, data, options, Restler.Method.PUT);

        private async Task<dynamic> _request(string url, object data, dynamic options, Restler.Method method, string contentType = null)
        {
            options = dyn.exp(options, true);
            if (!HttpTestExp.IsMatch(url))
            {
                var baseUrl = dyn.getProp(_options, "url", config.api_url);
                url = baseUrl + url;
            }
            _log($"Request: {url}");
            Func<Task<JObject>, Task<JObject>> requestFn = async (x) =>
            {
                if (!x.IsFaulted)
                {
                    var token = x.Result;
                    options.headers = dyn.getObj(options, "headers");
                    options.headers.Authorization = $"Bearer {token["access_token"]}";
                    try
                    {
                        var d = data == null ? (JObject)await _rest.request(url, options, method, contentType, (Action<HttpResponseMessage, string>)onResponse) : await _rest.json(url, data, options, method, (Action<HttpResponseMessage, string>)onResponse);
                        if (d["errors"] != null)
                        {
                            _log($"Request failed: {d}");
                            throw new MarketoException(HttpStatusCode.OK, d["errors"]);
                        }
                        return (dynamic)d;
                    }
                    catch (RestlerOperationException res)
                    {

                        var err = (JObject)res.Content;
                        if (err != null && err["errors"] != null) { _log($"Request failed: {err}"); throw; }
                        else
                        {
                            var e = res.E;
                            if (e != null) throw e;
                            else throw;
                            //var statusCode = Math.Floor((int)res.StatusCode / 100M);
                            //if (statusCode == 5) throw e;
                            //else if (statusCode == 4) throw e;
                            //else throw e;
                        }
                    }
                }
                else
                {
                    var err = x.Exception;
                    _log($"_request failed: {err}"); throw err;
                }
            };
            Func<bool, Task<JObject>> requestFn2 = async (forceOAuth) => await requestFn(GetOAuthToken(forceOAuth));
            return await _retry.start(requestFn2);

            void onResponse(HttpResponseMessage res, string body)
            {
                var headers = res.Headers;
                _lastApiUsage.Quota = headers.TryGetValues("X-Account-Quota", out IEnumerable<string> values) ? values.FirstOrDefault() : null;
                _lastApiUsage.QuotaUsed = headers.TryGetValues("X-Account-Quota-Used", out values) ? (int?)int.Parse(values.FirstOrDefault()) : null;
                OnResponse?.Invoke(this);
            }
        }

        private async Task<JObject> GetOAuthToken(bool force = false)
        {
            if (force || _tokenData == null)
            {
                Func<bool, Task<JObject>> requestFn = async (forceOAuth) =>
                {
                    var options = new
                    {
                        data = new
                        {
                            grant_type = "client_credentials",
                            client_id = dyn.getProp<string>(_options, "clientId"),
                            client_secret = dyn.getProp<string>(_options, "clientSecret"),
                        },
                        timeout = dyn.getProp(_options, "timeout", 20000),
                    };
                    var baseUrl = dyn.getProp(_options, "url", config.api_url);
                    try
                    {
                        var x = await _rest.post($"{baseUrl}/oauth/token", options);
                        var data = (JObject)x;

                        _log($"Got token: {data}");
                        _tokenData = data;
                        OnAccessToken?.Invoke(this);
                        return data;
                    }
                    catch (Exception err) { _log($"GetOAuthToken failed: {err}"); throw err; }
                };
                return await _retry.start(requestFn);
            }
            else
            {
                _log($"Using existing token: {_tokenData}");
                return _tokenData;
            }
        }
    }
}
