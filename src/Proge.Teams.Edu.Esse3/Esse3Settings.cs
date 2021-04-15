using System;

namespace Proge.Teams.Edu.Esse3
{
    /// <summary>
    /// Configuation for Esse3 HTTP Client
    /// </summary>
    public class Esse3Settings
    {
        public string WsBaseUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public const int MinimumRetryDelay = 1;
        private int _retryDelay = MinimumRetryDelay;
        public int RetryDelay { get => _retryDelay; set => _retryDelay = value >= MinimumRetryDelay ? value : MinimumRetryDelay; }
        public int MaxAttemptCount { get; set; }
    }
}
