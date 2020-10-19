using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Proge.Teams.Edu.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Proge.Teams.Edu.GraphApi.Models;

namespace Proge.Teams.Edu.GraphApi
{
    public interface ITeamsManager
    {
        Task Connect();
        Task<string> GetOrCreateJoinCode(string internalTeamsId);
        Task<string> GetJoinCode(string internalTeamId);
        Task<string> CreateJoinCode(string internalTeamId);
        Task<bool> ActivateTeam(string internalId);
    }

    public class TeamsManager : ITeamsManager
    {
        #region Variables and properties       
        private AuthenticationResult ccAuthenticationResult;
        private IPublicClientApplication unpApp { get; set; }
        private UsernamePasswordProvider authProvider { get; set; }

        private ILogger<TeamsManager> _logger;

        AuthenticationConfig _authenticationConfig { get; set; }
        private string unpToken { get; set; }
        private long unpExpires { get; set; }
        private string teamToken { get; set; }
        #endregion

        public TeamsManager(IOptions<AuthenticationConfig> authCfg, ILogger<TeamsManager> logger)
        {
            _logger = logger;
            _authenticationConfig = authCfg.Value;

            unpApp = PublicClientApplicationBuilder.Create(_authenticationConfig.ClientId)
                 .WithAuthority(AzureCloudInstance.AzurePublic, _authenticationConfig.TenantId)
                 .Build();
            authProvider = new UsernamePasswordProvider(unpApp);
        }

        /// <summary>
        /// Connect with username and pwd.
        /// </summary>
        /// <param name="config">Mapping of appsettings.json containing authentication input data.</param>
        /// <returns></returns>
        private async Task ConnectWithUnp()
        {
            //Teams url, DO NOT EDIT! Must be hardcoded
            var resourceAppIdURI = "https://teams.microsoft.com";
            //Client id for Teams API, DO NOT EDIT! Must be hardcoded
            var ClientIdTeams = "d3590ed6-52b3-4102-aeff-aad2292ab01c";
            string tokenEndpoint = $"https://login.microsoftonline.com/{_authenticationConfig.TenantId}/oauth2/token";

            using (var httpClient = new HttpClient())
            {
                var body = $"resource={resourceAppIdURI}&client_id={ClientIdTeams}&grant_type=password&username={_authenticationConfig.Username}&password={_authenticationConfig.Password}&authority=https://login.windows.net/{_authenticationConfig.TenantId}";
                var stringContent = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");

                var result = await httpClient.PostAsync(tokenEndpoint, stringContent);
                result.EnsureSuccessStatusCode();
                unpToken = await result.Content.ReadAsStringAsync();
                var test = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(unpToken);
                unpToken = test.access_token;
                unpExpires = test.expires_on;
            }
        }

        private async Task GetTeamToken()
        {
            var authUrl = $"https://teams.microsoft.com/api/authsvc/v1.0/authz";

            using (var httpClient = new HttpClient())
            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, authUrl))
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", unpToken);
                httpRequest.Headers.Add("ExpiresOn", $"{unpExpires}");
                var response = await httpClient.SendAsync(httpRequest);
                response.EnsureSuccessStatusCode();

                var tests = await response.Content.ReadAsStringAsync();
                var test = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(tests);
                teamToken = test.tokens.skypeToken;
            }
        }

        /// <summary>
        /// Get join code of an existing team, or create one (if it has still not been created).
        /// </summary>
        /// <param name="internalTeamId">Id of the team.</param>
        /// <returns>The join code.</returns>
        public async Task<string> GetOrCreateJoinCode(string internalTeamId)
        {
            var requestUri = $"https://teams.microsoft.com/api/mt/part/msft/beta/teams/{internalTeamId}/joinCode";
            using (var httpClient = new HttpClient())
            {
                // Attempt to get the join code, if it already exists.
                try
                {
                    using (var httpRequest = new HttpRequestMessage(HttpMethod.Get, requestUri))
                    {
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", unpToken);
                        httpRequest.Headers.Add("X-Skypetoken", teamToken);
                        httpRequest.Headers.Add("ExpiresOn", $"{unpExpires}");
                        var response = await httpClient.SendAsync(httpRequest);
                        response.EnsureSuccessStatusCode();

                        var ret = await response.Content.ReadAsStringAsync();
                        var joinCode = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(ret);
                        if (string.IsNullOrWhiteSpace(joinCode))
                            throw new Exception($"GetJoinCode: empty join code for team internal id {internalTeamId}");
                        return joinCode;
                    }
                }
                // Attempt to create the join code, if it does not exists.
                catch (Exception ex1)
                {
                    _logger.LogWarning(ex1, $"GetJoinCode: Get failed for team InternalId {internalTeamId}, probably it hasn't been generated. Start generation");
                    try
                    {
                        using (var httpRequest = new HttpRequestMessage(HttpMethod.Put, requestUri))
                        {
                            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", unpToken);
                            httpRequest.Headers.Add("X-Skypetoken", teamToken);
                            httpRequest.Headers.Add("ExpiresOn", $"{unpExpires}");
                            var response = await httpClient.SendAsync(httpRequest);
                            response.EnsureSuccessStatusCode();

                            var ret = await response.Content.ReadAsStringAsync();
                            var joinCode = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(ret);
                            if (string.IsNullOrWhiteSpace(joinCode))
                                throw new Exception($"GetJoinCode: Join code http call succes, but with empty result");
                            return joinCode;
                        }
                    }
                    // It creation attempt fails: not active team.
                    catch (Exception ex2)
                    {
                        _logger.LogWarning(ex1, $"GetJoinCode: Post failed for team InternalId {internalTeamId}, probably team hasn't been activated");
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// Get join code of an existing team.
        /// </summary>
        /// <param name="internalTeamId">Id of the team.</param>
        /// <returns>The join code.</returns>
        public async Task<string> GetJoinCode(string internalTeamId)
        {
            var requestUri = $"https://teams.microsoft.com/api/mt/part/msft/beta/teams/{internalTeamId}/joinCode";
            using (var httpClient = new HttpClient())
            {
                try
                {
                    var httpRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", unpToken);
                    httpRequest.Headers.Add("X-Skypetoken", teamToken);
                    httpRequest.Headers.Add("ExpiresOn", $"{unpExpires}");
                    var response = await httpClient.SendAsync(httpRequest);
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        //TODO: Refresh token
                        await Connect();
                        httpRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);
                        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", unpToken);
                        httpRequest.Headers.Add("X-Skypetoken", teamToken);
                        httpRequest.Headers.Add("ExpiresOn", $"{unpExpires}");
                        response = await httpClient.SendAsync(httpRequest);
                    }
                    response.EnsureSuccessStatusCode();

                    var ret = await response.Content.ReadAsStringAsync();
                    var joinCode = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(ret);
                    return joinCode;

                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex, $"GetJoinCode: Get failed for team InternalId {internalTeamId}.");
                    return null;
                }
            }
        }

        /// <summary>
        /// Create the join code for an existing team.
        /// </summary>
        /// <param name="internalTeamId">Id of the team.</param>
        /// <returns>The join code.</returns>
        public async Task<string> CreateJoinCode(string internalTeamId)
        {
            var requestUri = $"https://teams.microsoft.com/api/mt/part/msft/beta/teams/{internalTeamId}/joinCode";
            using (var httpClient = new HttpClient())
            {
                try
                {
                    var httpRequest = new HttpRequestMessage(HttpMethod.Put, requestUri);

                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", unpToken);
                    httpRequest.Headers.Add("X-Skypetoken", teamToken);
                    httpRequest.Headers.Add("ExpiresOn", $"{unpExpires}");
                    var response = await httpClient.SendAsync(httpRequest);
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        //TODO: Refresh token
                        await Connect();
                        httpRequest = new HttpRequestMessage(HttpMethod.Put, requestUri);
                        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", unpToken);
                        httpRequest.Headers.Add("X-Skypetoken", teamToken);
                        httpRequest.Headers.Add("ExpiresOn", $"{unpExpires}");
                        response = await httpClient.SendAsync(httpRequest);
                    }
                    response.EnsureSuccessStatusCode();

                    var ret = await response.Content.ReadAsStringAsync();
                    var joinCode = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(ret);
                    return joinCode;

                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"GetJoinCode: Post failed for team InternalId {internalTeamId}.");
                    return null;
                }
            }
        }

        /// <summary>
        /// Get the properties and relationships of the specified team.
        /// </summary>
        /// <param name="id">Id of the team.</param>
        /// <returns>Microsoft.Graph.Team object.</returns>
        public async Task<bool> ActivateTeam(string internalTeamId)
        {
            var requestUri = $"https://teams.microsoft.com/api/mt/part/msft/beta/teams/{internalTeamId}/unlock";
            using (var httpClient = new HttpClient())
            {
                try
                {
                    using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri))
                    {
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", unpToken);
                        httpRequest.Headers.Add("X-Skypetoken", teamToken);
                        httpRequest.Headers.Add("ExpiresOn", $"{unpExpires}");
                        var response = await httpClient.SendAsync(httpRequest);
                        response.EnsureSuccessStatusCode();

                        var ret = await response.Content.ReadAsStringAsync();

                        try
                        {
                            var result = JsonSerializer.Deserialize<TeamActivation>(ret, DefaultSerializerOption);
                            if (result == null || result.value == null || string.IsNullOrWhiteSpace(result.value.status))
                                return false;
                            else
                                return result.value.status == "Success" ? true : false;
                        }
                        catch (JsonException ex) // Invalid JSON
                        {
                            _logger.LogWarning(ex, $"ActivateTeam: Error deserializing the response for  {ret}");
                            return false;
                        }

                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"ActivateTeam: Error activating team with internalId {internalTeamId}");
                    return false;
                }
            }
        }

        private static JsonSerializerOptions DefaultSerializerOption = new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Establish the connection.
        /// </summary>
        /// <returns></returns>
        public async Task Connect()
        {
            await ConnectWithUnp();
            await GetTeamToken();
        }
    }
}
