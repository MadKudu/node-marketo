using System;
using System.Net;

namespace Marketo.Require
{
    public class RestlerOperationException : Exception
    {
        internal RestlerOperationException(HttpStatusCode statusCode, object content)
            : base(content != null ? content.ToString() : "Error")
        {
            StatusCode = statusCode;
            Content = content;
        }

        public HttpStatusCode StatusCode { get; set; }
        public object Content { get; set; }
        public Exception E { get; set; }
        public bool Timedout { get; set; }
    }
}