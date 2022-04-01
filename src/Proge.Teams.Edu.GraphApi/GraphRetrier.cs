using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace Proge.Teams.Edu.GraphApi
{
    public interface IRetrier
    {
        Task<T> Do<T>(Func<Task<T>> action, TimeSpan retryInterval, int maxAttemptCount = GraphRetrier.DefaultMaxAttemptCount, CancellationToken cancellationToken = default);
        Task Do(Func<Task> action, TimeSpan retryInterval, int maxAttemptCount = GraphRetrier.DefaultMaxAttemptCount, CancellationToken cancellationToken = default);
    }

    public interface IGraphRetrier : IRetrier { }

    public class GraphRetrier : IGraphRetrier 
    {
        public const int DefaultMaxAttemptCount = 3;
        private readonly HttpStatusCode[] RetriableStatusCodes = new HttpStatusCode[] {
            HttpStatusCode.NotFound,
            HttpStatusCode.MethodNotAllowed,
            HttpStatusCode.RequestTimeout,
            HttpStatusCode.BadRequest,
            //HttpStatusCode.Conflict,
            (HttpStatusCode)429, // TooManyRequest
        };
        private readonly ILogger<GraphRetrier> _logger;

        public GraphRetrier(ILogger<GraphRetrier> logger)
        {
            _logger = logger;
        }

        public async Task<T> Do<T>(
            Func<Task<T>> action,
            TimeSpan retryInterval,
            int maxAttemptCount = DefaultMaxAttemptCount
            , CancellationToken cancellationToken = default)
        {
            var exceptions = new List<Exception>();

            int attempt = 1;
            for (; attempt <= maxAttemptCount; attempt++)
            {
                try
                {
                    return await action();
                }
                catch (ServiceException ex) when (RetriableStatusCodes.Contains(ex.StatusCode))
                {
                    exceptions.Add(ex);
                    await LogAndWaitIfNeeded(ex, attempt, maxAttemptCount, retryInterval, cancellationToken);
                }
                catch (Exception) { throw; }
            }

            throw new AggregateException(exceptions);
        }

        public async Task Do(
            Func<Task> action,
            TimeSpan retryInterval,
            int maxAttemptCount = DefaultMaxAttemptCount, 
            CancellationToken cancellationToken = default)
        {
            var exceptions = new List<Exception>();

            int attempt = 1;
            for (; attempt <= maxAttemptCount; attempt++)
            {
                try
                {
                    await action();
                    return;
                }
                catch (ServiceException ex) when (RetriableStatusCodes.Contains(ex.StatusCode))
                {
                    exceptions.Add(ex);
                    await LogAndWaitIfNeeded(ex, attempt, maxAttemptCount, retryInterval, cancellationToken);
                }
                catch (Exception) { throw; }
            }

            throw new AggregateException(exceptions);
        }

        private async Task LogAndWaitIfNeeded(ServiceException ex, int attempt, int maxAttemptCount, TimeSpan retryInterval,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Service exception with StatusCode {0} at attempt {1}/{2}", ex.StatusCode, attempt, maxAttemptCount);
            _logger.LogTrace(ex, "Service exception with StatusCode {0} at attempt {1}/{2}", ex.StatusCode, attempt, maxAttemptCount);

            if (attempt == maxAttemptCount)
            {
                _logger.LogDebug("Maximum number of attempt tried, won't try again");
                return;
            }

            TimeSpan timeToWait = TimeToWait(attempt, retryInterval);
            _logger.LogDebug("Waiting {0}ms before next attempt", timeToWait.TotalMilliseconds);
            await Task.Delay(timeToWait, cancellationToken);
        }

        private TimeSpan TimeToWait(int attempt, TimeSpan retryInterval)
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
