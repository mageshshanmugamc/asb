namespace ASB.ErrorHandler.v1.Exceptions
{
    public class UnprocessableEntityException : Exception
    {
        public UnprocessableEntityException() { }

        public UnprocessableEntityException(string message) : base(message) { }

        public UnprocessableEntityException(string message, Exception innerException) : base(message, innerException) { }
    }   
}