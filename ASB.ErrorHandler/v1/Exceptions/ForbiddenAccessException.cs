using System;
using System.Collections.Generic;
using System.Text;

namespace ASB.ErrorHandler.v1.Exceptions
{
    /// <summary>
    /// ForbiddenAccessException is thrown when a user tries to access a resource or perform an action that they do not have permission for. This typically corresponds to HTTP status code 403 Forbidden. It indicates that the server understands the request but refuses to authorize it, often due to insufficient permissions or access rights.
    /// </summary>
    public class ForbiddenAccessException : Exception
    {
        public ForbiddenAccessException() { }

        public ForbiddenAccessException(string message) : base(message) { }

        public ForbiddenAccessException(string message, Exception innerException) : base(message, innerException) { }
    }
}
