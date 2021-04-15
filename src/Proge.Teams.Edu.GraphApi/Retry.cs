using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace Proge.Teams.Edu.GraphApi
{
    public static class Retry
    {
        public const int DefaultMaxAttemptCount = 5;

        public static readonly HttpStatusCode[] RetriableStatusCodes = new HttpStatusCode[] {
            HttpStatusCode.NotFound,
            HttpStatusCode.MethodNotAllowed,
            HttpStatusCode.RequestTimeout,
            (HttpStatusCode)429, // TooManyRequest
        };

        public static async Task<T> Do<T>(
            Func<Task<T>> action,
            TimeSpan retryInterval,
            int maxAttemptCount = DefaultMaxAttemptCount)
        {
            var exceptions = new List<Exception>();

            int attempt = 1;
            for (; attempt <= maxAttemptCount; attempt++)
            {
                try
                {
                    return await action();
                }
                catch (ClientException ex) when (RetriableStatusCodes.Contains(ex.StatusCode))
                {
                    exceptions.Add(ex);
                    TimeSpan timeToWait = TimeToWait(attempt, retryInterval);
                    await Task.Delay(timeToWait);
                }
                catch (Exception) { throw; }
            }

            throw new AggregateException(exceptions);
        }

        public static async Task Do(
            Func<Task> action,
            TimeSpan retryInterval,
            int maxAttemptCount = DefaultMaxAttemptCount)
        {
            var exceptions = new List<Exception>();

            int attempt = 1;
            for (; attempt <= maxAttemptCount; attempt++)
            {
                try
                {
                    await action();
                    break;
                }
                catch (ClientException ex) when (RetriableStatusCodes.Contains(ex.StatusCode))
                {
                    exceptions.Add(ex);
                    TimeSpan timeToWait = TimeToWait(attempt, retryInterval);
                    await Task.Delay(timeToWait);
                }
                catch (Exception) { throw; }
            }

            throw new AggregateException(exceptions);
        }


        private static TimeSpan TimeToWait(int attempt, TimeSpan retryInterval)
        {
            if (attempt <= 0)
                throw new ArgumentException($"Attempt must be a positive integer; given {attempt}");
            if (retryInterval.TotalSeconds <= 0)
                throw new ArgumentException($"Retry Interval must be a positive integer; given {retryInterval.TotalSeconds}");

            double retryDelay = retryInterval.TotalSeconds;
            double secToWait = retryDelay * Math.Pow(2, attempt);
            TimeSpan timeToWait = TimeSpan.FromSeconds(secToWait);
            return timeToWait;
        }
    }
}
