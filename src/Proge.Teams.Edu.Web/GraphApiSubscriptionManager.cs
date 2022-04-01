using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Proge.Teams.Edu.Abstraction;
using Proge.Teams.Edu.TeamsDashboard;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.Web
{
    /// <summary>
    /// Renews the Subscription to MS Graph API for events about MS Teams meeting
    /// </summary>
    public interface IGraphApiSubscriptionManager
    {
        Task Run();
        Task DeleteSubscription();
    }

    public class GraphApiSubscriptionManager : IGraphApiSubscriptionManager
    {
        private readonly IGraphApiManager _graphApiManager;
        private readonly ITeamsDataCollectorManager _teamsDataCollectorManager;
        private readonly ILogger<GraphApiSubscriptionManager> _logger;
        private readonly UniSettings _uniSettings;

        public GraphApiSubscriptionManager(
            IGraphApiManager graphApiManager,
            IOptions<UniSettings> uniSettings,
            ITeamsDataCollectorManager teamsDataCollector,
            ILogger<GraphApiSubscriptionManager> logger)
        {
            _teamsDataCollectorManager = teamsDataCollector;
            _graphApiManager = graphApiManager;
            _logger = logger;
            _uniSettings = uniSettings.Value;
        }

        public async Task DeleteSubscription()
        {
            try
            {
                _logger.LogInformation("Renewing Subscription");
                var subscription = await _graphApiManager.GetSubscriptions();
                var currentSubscription = subscription.FirstOrDefault(a =>
                a.Resource == _uniSettings.Resource
                && a.NotificationUrl == _uniSettings.NotificationUrl);

                if (currentSubscription != null)
                {
                    await _graphApiManager.DeleteSubscription(currentSubscription.Id);
                }
                else
                {
                    throw new ArgumentOutOfRangeException("No subscription found");
                }

                _logger.LogInformation("Subscription renewed");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error renewing the Graph API Subscription");
                throw;
            }
        }

        public async Task Run()
        {
            try
            {
                _logger.LogInformation("Renewing Subscription");
                await _teamsDataCollectorManager.SubscribeCallRecords();
                _logger.LogInformation("Subscription renewed");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error renewing the Graph API Subscription");
                throw;
            }
        }
    }
}