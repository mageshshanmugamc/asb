using System;
using System.Collections.Generic;
using System.Text;

namespace ASB.ErrorHandler.v1.Exceptions
{
    /// <summary>
    /// ConflictException is thrown when a conflict occurs, such as a resource already existing or a version mismatch. This typically corresponds to HTTP status code 409 Conflict. It indicates that the request could not be completed due to a conflict with the current state of the resource.
    /// </summary>
    public class ConflictException : Exception
    {
        public ConflictException() { }

        public ConflictException(string message) : base(message) { }

        public ConflictException(string message, Exception innerException) : base(message, innerException) { }
    }
}
