namespace Rag.Services.Backend.Domain.Exceptions
{
    public abstract class BackendException : Exception
    {
        protected BackendException() {}

        protected BackendException(string message)
            : base(message)
        {
        }

        protected BackendException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}