using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Proge.Teams.Edu.Esse3
{
    public interface IRetryManager
    {
        Task<HttpResponseMessage> DoSendAsyncRequest<T>(HttpClient client, HttpMethod method, string url, string jSession, params string[] queryString);
    }

    public class RetryManager : IRetryManager
    {
        private readonly Esse3Settings _esse3Settings;
        private readonly ILogger<RetryManager> _logger;

        public RetryManager(
            IOptions<Esse3Settings> settings,
            ILogger<RetryManager> logger)
        {
            _esse3Settings = settings.Value;
            _logger = logger;
        }

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
                _logger.LogDebug("Performing ({0}, {1}) {2} to {3}", attempt, maxAttempt, method, requestMessage.RequestUri);

                HttpResponseMessage res = null;
                try
                {
                    res = await PerformRequest(client, requestMessage, attempt, maxAttempt);
                    return res;
                }
                catch (Exception) when (res?.StatusCode != (HttpStatusCode)429) { throw; }
                catch (Exception ex)
                {
                    // store exception from 429
                    exceptions.Add(ex);

                    // wait until next retry
                    TimeSpan timeToWait = TimeToWait(attempt);
                    await Task.Delay(timeToWait);
                }
            }

            throw new AggregateException(exceptions);
        }

        private async Task<HttpResponseMessage> PerformRequest(HttpClient client, HttpRequestMessage requestMessage, int attempt, int maxAttempt)
        {
            HttpResponseMessage res = null;
            try
            {
                res = await client.SendAsync(requestMessage);
                res.EnsureSuccessStatusCode();
                return res;
            }
            catch (Exception ex)
            {
                // log
                string headers = string.Join(";", res?.Content?.Headers?.Select(a => $"Key:{a.Key}-Value:{string.Join(",", a.Value)}"));
                _logger.LogError(ex, "Retry {0}/{1}. Status Code: {2}. Response headers: {3}",
                    attempt, maxAttempt, res?.StatusCode.ToString() ?? "null response", headers);
                throw;
            }
        }

        private TimeSpan TimeToWait(int attempt)
        {
            if (attempt <= 0)
                throw new ArgumentException($"Attempt must be a positive integer; given {attempt}");

            int retryDelay = _esse3Settings.RetryDelay;
            double secToWait = retryDelay * Math.Pow(2, attempt);
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
