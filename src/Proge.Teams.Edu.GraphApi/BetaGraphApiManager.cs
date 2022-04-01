extern alias BetaLib;
using Beta = BetaLib.Microsoft.Graph;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Proge.Teams.Edu.Abstraction;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Graph;
using System.Net.Http.Headers;
using System.Linq;

namespace Proge.Teams.Edu.GraphApi
{
    public class BetaGraphApiManager : IBetaGraphApiManager
    {
        private readonly ILogger<BetaGraphApiManager> _logger;
        private IGraphRetrier _retrier { get; set; }
        private readonly AuthenticationConfig _authenticationConfig;
        private IConfidentialClientApplication app { get; set; }
        private Beta.GraphServiceClient graphClient { get; set; }

        public BetaGraphApiManager(IOptions<AuthenticationConfig> authCfg,
            IGraphRetrier retrier,
            ILogger<BetaGraphApiManager> logger)
        {
            _logger = logger;
            _retrier = retrier;
            _authenticationConfig = authCfg.Value;
            app = ConfidentialClientApplicationBuilder.Create(_authenticationConfig.ClientId)
                  .WithAuthority(AzureCloudInstance.AzurePublic, _authenticationConfig.TenantId)
                  .WithClientSecret(_authenticationConfig.ClientSecret)
                  .Build();
            graphClient = new Beta.GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
            {
                _logger.LogTrace("Requesting new access token to Microsoft Graph API");
                var authResult = await app
                    .AcquireTokenForClient(new string[1] { "https://graph.microsoft.com/.default" })
                    .ExecuteAsync();

                _logger.LogTrace("Add access token to request message");
                requestMessage
                    .Headers
                    .Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
            }));
        }

        public async Task<Beta.OnlineMeeting> GetOnlineMeeting(string userId, string joinWebUrl)
        {
            try
            {
                string filter = string.Format("JoinWebUrl%20eq%20'{0}'", joinWebUrl);

                var onlineMeetingsRequest = graphClient.Users[userId].OnlineMeetings
                    .Request()
                    .Filter(filter);
                var onlineMeetings = await onlineMeetingsRequest.GetAsync();
                var json = System.Text.Json.JsonSerializer.Serialize(onlineMeetings);
                return onlineMeetings.CurrentPage.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetOnlineMeeting: ");
                return null;
            }
        }

        public async Task<Beta.Subscription> AddSubscription(string changeType, string resource, DateTimeOffset? expirationOffset, string clientStateSecret,
             string notificationUrl)
        {
            try
            {
                var subscription = new Beta.Subscription()
                {
                    ChangeType = changeType,
                    Resource = resource,
                    ExpirationDateTime = expirationOffset,
                    ClientState = clientStateSecret,
                    NotificationUrl = notificationUrl,

                };

                var res = await graphClient.Subscriptions
                    .Request()
                    .AddAsync(subscription)
                    ;

                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RenewSubscription: ");
                return null;
            }
        }


        public async Task RenewSubscription(string id)
        {
            try
            {
                await graphClient.Subscriptions[id].Request().UpdateAsync(new Beta.Subscription
                {
                    ExpirationDateTime = new DateTimeOffset(DateTime.UtcNow.AddDays(2), TimeSpan.Zero)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RenewSubscription: ");
            }
        }
        public async Task<bool> GetSubscriptions()
        {
            try
            {
                bool isRenewed = false;
                Beta.IGraphServiceSubscriptionsCollectionPage subscriptionCollectionPage = await graphClient.Subscriptions.Request().GetAsync();
                Beta.IGraphServiceSubscriptionsCollectionRequest subscriptionsNextpageRequest = subscriptionCollectionPage.NextPageRequest;

                foreach (Beta.Subscription subscription in subscriptionCollectionPage.CurrentPage)
                {
                    await RenewSubscription(subscription.Id);
                    isRenewed = true;
                }

                while (subscriptionsNextpageRequest != null)
                {
                    Beta.IGraphServiceSubscriptionsCollectionPage subscriptionsNextPage = await subscriptionsNextpageRequest.GetAsync();
                    subscriptionsNextpageRequest = subscriptionsNextPage.NextPageRequest;

                    foreach (Beta.Subscription subscription in subscriptionsNextPage.CurrentPage)
                    {
                        await RenewSubscription(subscription.Id);
                        isRenewed = true;
                    }
                }

                return isRenewed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetSubscriptions: ");
                return false;
            }
        }


        public async Task<System.IO.Stream> GetTeamsUserActivityUserDetail(Microsoft.Graph.Date date)
        {
            var res = await graphClient.Reports.GetTeamsUserActivityUserDetail(date)
                .Request()
                .GetAsync()
                ;

            return res;
        }

        public async Task<Beta.CallRecords.CallRecord> GetCallRecord(string callId)
        {
            try
            {
                var res = await graphClient.Communications.CallRecords[$"{callId}?$expand=sessions($expand=segments)"]
                .Request()
                .GetAsync();

                return res;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"GetCallRecord: CallRecord *probably* not found");
                return null;
            }
        }

        /// <summary>
        /// Get the properties and relationships of the specified team.
        /// </summary>
        /// <param name="id">Id of the team.</param>
        /// <returns>Microsoft.Graph.Team object.</returns>
        public async Task<Beta.Team> GetTeam(string id)
        {
            try
            {
                var resGroup = await _retrier.Do(async () => await graphClient.Teams[$"{id}"]
                        .Request()
                        .GetAsync(), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
                return resGroup;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"GetTeam: Team *probably* not found");
                return null;
            }
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
