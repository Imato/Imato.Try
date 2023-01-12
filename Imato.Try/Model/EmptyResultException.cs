using System;

namespace Imato.Try
{
    public class EmptyResultException : ApplicationException
    {
        public EmptyResultException() : base("Empty result")
        {
        }
    }
}