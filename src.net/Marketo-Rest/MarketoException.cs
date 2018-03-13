using Newtonsoft.Json.Linq;
using System;
using System.Net;

namespace Marketo
{
    public class MarketoException : Exception
    {
        internal MarketoException(string message)
            : this(HttpStatusCode.OK, null, -1, message) { }
        internal MarketoException(HttpStatusCode statusCode, JToken errors)
            : this(statusCode, (string)errors[0]["id"], (int)errors[0]["code"], (string)errors[0]["message"]) { }
        internal MarketoException(HttpStatusCode statusCode, string id, int code, string message)
            : base(message)
        {
            StatusCode = statusCode;
            Id = id;
            Code = code;
        }

        public HttpStatusCode StatusCode { get; set; }
        public string Id { get; set; }
        public int Code { get; set; }
    }

    public class MarketoSecurityException : MarketoException
    {
        internal MarketoSecurityException(HttpStatusCode statusCode, JToken errors)
            : base(statusCode, errors) { }
    }
}