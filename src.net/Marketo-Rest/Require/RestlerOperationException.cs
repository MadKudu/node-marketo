using System;
using System.Net;

namespace Marketo.Require
{
    /// <summary>
    /// Class RestlerOperationException.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class RestlerOperationException : Exception
    {
        internal RestlerOperationException(HttpStatusCode statusCode, object content)
            : base(content != null ? content.ToString() : "Error")
        {
            StatusCode = statusCode;
            Content = content;
        }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        /// <value>The status code.</value>
        public HttpStatusCode StatusCode { get; set; }
        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>The content.</value>
        public object Content { get; set; }
        /// <summary>
        /// Gets or sets the e.
        /// </summary>
        /// <value>The e.</value>
        public Exception E { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RestlerOperationException"/> is timedout.
        /// </summary>
        /// <value><c>true</c> if timedout; otherwise, <c>false</c>.</value>
        public bool Timedout { get; set; }
    }
}