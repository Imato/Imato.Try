namespace Imato.Try
{
    public class TryOptions
    {
        public int RetryCount { get; set; } = 1;

        /// <summary>
        /// Delay between retry
        /// </summary>
        public int Delay { get; set; } = 0;

        /// <summary>
        /// Create exception
        /// </summary>
        public bool ErrorOnFail { get; set; } = true;

        public int Timeout { get; set; } = 30000;
    }
}