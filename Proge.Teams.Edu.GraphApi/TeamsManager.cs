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

namespace Proge.Teams.Edu.GraphApi
{
    public interface ITeamsManager
    {
        Task Connect();
        Task<string> GetJoinCode(string internalTeamsId);
    }

    public class TeamsManager : ITeamsManager
    {
        #region Variables and properties       
        private AuthenticationResult ccAuthenticationResult;
        private IPublicClientApplication unpApp { get; set; }
        private UsernamePasswordProvider authProvider { get; set; }
        AuthenticationConfig _authenticationConfig { get; set; }
        private string unpToken { get; set; }
        private long unpExpires { get; set; }
        private string teamToken { get; set; }
        #endregion

        public TeamsManager(IOptions<AuthenticationConfig> authCfg)
        {
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

        public async Task<string> GetJoinCode(string internalTeamsId)
        {
            var requestUri = $"https://teams.microsoft.com/api/mt/part/msft/beta/teams/{internalTeamsId}/joinCode";
            using (var httpClient = new HttpClient())
            {
                //Provo a prendere il join code se esiste
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
                        return ret;
                    }
                }
                //Se non esiste provo a generarlo
                catch (Exception ex1)
                {
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
                            return ret;
                        }
                    }
                    //Se non riesco a generalo, il team non è attivo
                    catch (Exception ex2)
                    {
                        return null;
                    }
                }

            }
        }

        public async Task Connect()
        {
            await ConnectWithUnp();
            await GetTeamToken();
        }
    }
}
