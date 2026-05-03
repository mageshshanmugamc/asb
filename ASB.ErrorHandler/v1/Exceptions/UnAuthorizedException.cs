using System;
using System.Collections.Generic;
using System.Text;

namespace ASB.ErrorHandler.v1.Exceptions
{
    public class UnAuthorizedException : Exception
    {
        public UnAuthorizedException() { }

        public UnAuthorizedException(string message) : base(message) { }

        public UnAuthorizedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
