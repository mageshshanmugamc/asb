using System;
using System.Collections.Generic;
using System.Text;

namespace ASB.ErrorHandler.v1.Exceptions
{
    /// <summary>
    /// PaymentRequiredException is thrown when a user tries to access a resource or perform an action that requires payment. This typically corresponds to HTTP status code 402 Payment Required. It indicates that the server understands the request but requires payment before granting access.
    /// </summary>
    public class PaymentRequiredException : Exception
    {
        public PaymentRequiredException() { }

        public PaymentRequiredException(string message) : base(message) { }

        public PaymentRequiredException(string message, Exception innerException) : base(message, innerException) { }
    }
}
