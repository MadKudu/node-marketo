using Newtonsoft.Json.Linq;
using System;
using System.Net;

namespace Marketo
{
    /// <summary>
    /// Class MarketoException.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class MarketoException : Exception
    {
        internal MarketoException(string message)
            : this(HttpStatusCode.OK, null, -1, message) { }
        internal MarketoException(HttpStatusCode statusCode, JArray errors)
            : this(statusCode, (string)errors[0]["id"], (int)errors[0]["code"], (string)errors[0]["message"]) { Errors = errors; }
        internal MarketoException(HttpStatusCode statusCode, string id, int code, string message)
            : base(message)
        {
            StatusCode = statusCode;
            Id = id;
            Code = code;
        }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        /// <value>The status code.</value>
        public HttpStatusCode StatusCode { get; set; }
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>The code.</value>
        public int Code { get; set; }
        /// <summary>
        /// Gets or sets the errors.
        /// </summary>
        /// <value>The errors.</value>
        public JArray Errors { get; set; }
    }

    /// <summary>
    /// Class MarketoSecurityException.
    /// </summary>
    /// <seealso cref="Marketo.MarketoException" />
    public class MarketoSecurityException : MarketoException
    {
        internal MarketoSecurityException(HttpStatusCode statusCode, JArray errors)
            : base(statusCode, errors) { }
    }
}