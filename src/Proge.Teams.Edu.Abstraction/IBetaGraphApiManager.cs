extern alias BetaLib;
using System;
using System.Threading.Tasks;
using Beta = BetaLib.Microsoft.Graph;

namespace Proge.Teams.Edu.Abstraction
{
    public interface IBetaGraphApiManager
    {
        Task ConnectAsApplication();
        Beta.Team DefaultTeamFactory(string groupId);
        Task<Beta.Team> GetTeam(string id);
        Task<Beta.Subscription> AddSubscription(string changeType, string resource, DateTimeOffset? expirationOffset, string clientStateSecret, string notificationUrl);
        Task RenewSubscription(string id);
        Task<bool> GetSubscriptions();
    }
}
