using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Proge.Teams.Edu.Esse3
{
    public interface IRetryManager
    {
        Task<HttpResponseMessage> DoSendAsyncRequest<T>(HttpClient client, HttpMethod method, string url, string jSession, params string[] queryString);
        Task<HttpResponseMessage> DoSendAsyncRequest<T>(HttpClient client, HttpMethod method, string url, string jSession, string[] queryString, CancellationToken cancellationToken = default);
    }

    public class RetryManager : IRetryManager
    {
        private readonly Esse3Settings _esse3Settings;
        private readonly ILogger<RetryManager> _logger;
        private readonly Random _random;

        public RetryManager(
            IOptions<Esse3Settings> settings,
            ILogger<RetryManager> logger)
        {
            _esse3Settings = settings.Value;
            _logger = logger;
            _random = new Random();
        }


        public async Task<HttpResponseMessage> DoSendAsyncRequest<T>(
            HttpClient client,
            HttpMethod method,
            string url,
            string jSession,
            string[] queryString,
            CancellationToken cancellationToken = default)
        {
            var exceptions = new List<Exception>();

            int attempt = 1, maxAttempt = _esse3Settings.MaxAttemptCount;
            for (; attempt <= maxAttempt; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                HttpRequestMessage requestMessage = RequestMessageFactory(method, _esse3Settings, jSession, url, queryString);
                _logger.LogTrace("Performing ({0}, {1}) {2} to {3}", attempt, maxAttempt, method, requestMessage.RequestUri);

                HttpResponseMessage res = null;
                try
                {
                    res = await client.SendAsync(requestMessage, cancellationToken);

                    res.EnsureSuccessStatusCode();
                    return res;
                }
                catch (Exception ex) when (res?.StatusCode == (HttpStatusCode)429)
                {
                    LogErrorResponse(ex, res, attempt, maxAttempt);

                    // store exception from 429
                    exceptions.Add(ex);

                    if (attempt <= maxAttempt)
                    {
                        // wait until next retry
                        TimeSpan timeToWait = TimeToWait(attempt);
                        _logger.LogInformation("Waiting {0} seconds before retrying request with 429 response", timeToWait);
                        await Task.Delay(timeToWait, cancellationToken);
                    }
                }
            }

            throw new AggregateException(exceptions);
        }

        [Obsolete]
        public async Task<HttpResponseMessage> DoSendAsyncRequest<T>(
            HttpClient client,
            HttpMethod method,
            string url,
            string jSession,
            params string[] queryString)
        {
            var exceptions = new List<Exception>();

            int attempt = 1, maxAttempt = _esse3Settings.MaxAttemptCount;
            for (; attempt <= maxAttempt; attempt++)
            {
                HttpRequestMessage requestMessage = RequestMessageFactory(method, _esse3Settings, jSession, url, queryString);
                _logger.LogTrace("Performing ({0}, {1}) {2} to {3}", attempt, maxAttempt, method, requestMessage.RequestUri);

                HttpResponseMessage res = null;
                try
                {
                    res = await client.SendAsync(requestMessage);

                    res.EnsureSuccessStatusCode();
                    return res;
                }
                catch (Exception ex) when (res?.StatusCode == (HttpStatusCode)429)
                {
                    LogErrorResponse(ex, res, attempt, maxAttempt);

                    // store exception from 429
                    exceptions.Add(ex);

                    if (attempt <= maxAttempt)
                    {
                        // wait until next retry
                        TimeSpan timeToWait = TimeToWait(attempt);
                        _logger.LogInformation("Waiting {0} seconds before retrying request with 429 response", timeToWait);
                        await Task.Delay(timeToWait);
                    }
                }
            }

            throw new AggregateException(exceptions);
        }

        private void LogErrorResponse(Exception ex, HttpResponseMessage res, int attempt, int maxAttempt)
        {
            static LogLevel getLogLevel(int a, int ma)
            {
                if (a <= 3 && ma > 3)
                    return LogLevel.Debug;

                if (a == ma)
                    return LogLevel.Error;

                return LogLevel.Warning;
            }

            string headers = string.Join(";", res?.Content?.Headers?.Select(a => $"Key:{a.Key}-Value:{string.Join(",", a.Value)}"));
            LogLevel level = getLogLevel(attempt, maxAttempt);
            _logger.Log(level, ex, "Retry {0}/{1}. Status Code: {2}. Response headers: {3}",
                attempt, maxAttempt, res?.StatusCode.ToString() ?? "null response", headers);
        }

        private TimeSpan TimeToWait(int attempt)
        {
            if (attempt <= 0)
                throw new ArgumentException($"Attempt must be a positive integer; given {attempt}");

            int retryDelay = _esse3Settings.RetryDelay;
            double rand = ((double)_random.Next(80, 100))/100;
            double secToWait = retryDelay * Math.Pow(2, attempt) * rand;
            TimeSpan timeToWait = TimeSpan.FromSeconds(secToWait);
            return timeToWait;
        }

        private HttpRequestMessage RequestMessageFactory(HttpMethod httpMethod, Esse3Settings esse3Settings, string jSession, string url, params string[] queryString)
        {
            var authenticationString = $"{esse3Settings.Username}:{esse3Settings.Password}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(authenticationString));

            var requestMessage = new HttpRequestMessage(httpMethod, PostPendSessionId(jSession, url, queryString));
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
            return requestMessage;
        }

        private string PostPendSessionId(string jSession, string url, params string[] queryString)
        {
            return $"{url}/;jsessionid={jSession}{(queryString == null || !queryString.Any() ? string.Empty : "?" + string.Join("&", queryString))}";
        }
    }
}
