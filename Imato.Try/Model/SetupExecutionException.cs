using System;

namespace Imato.Try
{
    public class SetupExecutionException : ApplicationException
    {
        public SetupExecutionException() : base()
        {
        }

        public SetupExecutionException(string? message) : base(message)
        {
        }

        public SetupExecutionException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}