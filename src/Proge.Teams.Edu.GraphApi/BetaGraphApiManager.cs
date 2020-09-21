extern alias BetaLib;
using Beta = BetaLib.Microsoft.Graph;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Proge.Teams.Edu.Abstraction;
using Proge.Teams.Edu.GraphApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;


namespace Proge.Teams.Edu.GraphApi
{
    public class BetaGraphApiManager : IBetaGraphApiManager
    {
        private readonly AuthenticationConfig _authenticationConfig;
        private IConfidentialClientApplication app { get; set; }
        private ClientCredentialProvider authProvider { get; set; }
        private Beta.GraphServiceClient graphClient { get; set; }
        private AuthenticationResult authenticationResult;

        public BetaGraphApiManager(IOptions<AuthenticationConfig> authCfg)
        {
            //_mapper = mapper;
            _authenticationConfig = authCfg.Value;
            app = ConfidentialClientApplicationBuilder.Create(_authenticationConfig.ClientId)
                  .WithAuthority(AzureCloudInstance.AzurePublic, _authenticationConfig.TenantId)
                  .WithClientSecret(_authenticationConfig.ClientSecret)
                  .Build();
            authProvider = new ClientCredentialProvider(app);
            graphClient = new Beta.GraphServiceClient(authProvider);
        }

        /// <summary>
        /// Establish the connection.
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsApplication()
        {
            try
            {
                // Login Azure AD
                authenticationResult = await app.AcquireTokenForClient(_authenticationConfig.ScopeList)
                    .ExecuteAsync();
            }
            catch (MsalServiceException ex)
            {
                // Case when ex.Message contains:
                // AADSTS70011 Invalid scope. The scope has to be of the form "https://resourceUrl/.default"
                // Mitigation: change the scope to be as expected
                throw ex;
            }
        }

        /// <summary>
        /// Get the properties and relationships of the specified team.
        /// </summary>
        /// <param name="id">Id of the team.</param>
        /// <returns>Microsoft.Graph.Team object.</returns>
        public async Task<Beta.Team> GetTeam(string id)
        {
            var resGroup = await Retry.Do(async () => await graphClient.Teams[$"{id}"]
                .Request()
                .GetAsync(), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
            return resGroup;
        }

        /// <summary>
        /// Build a team from an existing group.
        /// </summary>
        /// <param name="groupId">Id of the existing group.</param>
        /// <returns>Microsoft.Graph.Team object.</returns>
        public Beta.Team DefaultTeamFactory(string groupId)
        {
            return new Beta.Team
            {
                ODataType = null,
                IsMembershipLimitedToOwners = true,
                AdditionalData = new Dictionary<string, object>()
                {
                    { "template@odata.bind", "https://graph.microsoft.com/beta/teamsTemplates('educationClass')"},
                    {"group@odata.bind", $"https://graph.microsoft.com/v1.0/groups('{groupId}')"}
                },
                GuestSettings = new Beta.TeamGuestSettings()
                {
                    AllowCreateUpdateChannels = false,
                    AllowDeleteChannels = false,
                    ODataType = null,
                },
                MemberSettings = new Beta.TeamMemberSettings
                {
                    ODataType = null,
                    AllowDeleteChannels = false,
                    AllowCreateUpdateChannels = false,
                    AllowAddRemoveApps = false,
                    AllowCreateUpdateRemoveTabs = false,
                    AllowCreateUpdateRemoveConnectors = false,
                },
                MessagingSettings = new Beta.TeamMessagingSettings
                {
                    ODataType = null,
                    AllowUserEditMessages = false,
                    AllowUserDeleteMessages = false,
                    AllowOwnerDeleteMessages = true,
                },
                FunSettings = new Beta.TeamFunSettings
                {
                    ODataType = null,
                    AllowGiphy = true,
                    GiphyContentRating = Beta.GiphyRatingType.Strict
                }
            };
        }
    }
}
