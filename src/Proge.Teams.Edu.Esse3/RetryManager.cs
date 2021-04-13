using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

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
        public RetryManager(ILogger<RetryManager> logger, IOptions<Esse3Settings> unimoresettings)
        {
            _esse3Settings = unimoresettings.Value;
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
            int retryCount = 1;
            TimeSpan retryInterval = TimeSpan.FromSeconds(_esse3Settings.RetryDelay);
            for (int attempted = 1; attempted <= _esse3Settings.MaxAttemptCount; attempted++)
            {
                retryCount = attempted;
                HttpResponseMessage res = null;
                try
                {
                    if (attempted > 1)
                        await Task.Delay(retryInterval);
                    else
                        await Task.Delay(TimeSpan.FromMilliseconds(300));

                    var requestMessage = RequestMessageFactory(method, _esse3Settings, jSession, url, queryString);
                    _logger.LogDebug($"Esse3 Client Request: {requestMessage.RequestUri}");
                    res = await client.SendAsync(requestMessage);
                    res.EnsureSuccessStatusCode();
                    return res;
                }
                catch (Exception ex)
                {
                    var r = res != null && res.Content != null && res.Content.Headers != null 
                        ? string.Join(";", res.Content.Headers.Select(a => $"Key:{a.Key}-Value:{string.Join(",",a.Value)}")) 
                        : string.Empty;
                    _logger.LogError(ex, $"Esse3 Client Request: Retry {attempted} of {_esse3Settings.MaxAttemptCount}. Status Code: {(res != null ? res.StatusCode.ToString() :"null response" ) }. Response header: {r}");
                    if (res != null && res.StatusCode == (System.Net.HttpStatusCode)429)
                        exceptions.Add(ex);
                    else
                        throw ex;
                }
            }
            if (retryCount >= _esse3Settings.MaxAttemptCount)
                throw new AggregateException(exceptions);
            else
                return default;
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
