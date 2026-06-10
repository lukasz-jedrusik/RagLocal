namespace Rag.Services.Backend.Domain.Exceptions
{
    public class BackendException : Exception
    {
        public BackendException()
        {
        }

        public BackendException(string message)
            : base(message)
        {
        }

        public BackendException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}