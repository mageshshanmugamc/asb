using System;
using System.Collections.Generic;
using System.Text;

namespace ASB.ErrorHandler.v1.Exceptions
{
    /// <summary>
    /// UnSupportedMediaTypeException is thrown when a request is made with an unsupported media type. This typically corresponds to HTTP status code 415 Unsupported Media Type. It indicates that the server refuses to accept the request because the payload format is not supported.
    /// </summary>
    public class UnSupportedMediaTypeException : Exception
    {
        public UnSupportedMediaTypeException() { }

        public UnSupportedMediaTypeException(string message) : base(message) { }

        public UnSupportedMediaTypeException(string message, Exception innerException) : base(message, innerException) { }
    }
}
