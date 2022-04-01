using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Proge.Teams.Edu.Abstraction;

namespace Proge.Teams.Edu.TeamsDashboard.ChangeNotificationSubscription
{
    public record struct SubscriptionData
    {
        public string ChangeType;
        public string Resource;
        public string ClientStateSecret;
        public string NotificationUrl;
        public DateTimeOffset NewExpiration;
    }

    public interface IChangeNotificationSubscriptionManager
    {
        Task<Subscription> Retrieve(string resource, string notificationUrl, CancellationToken cancellationToken = default);
        Task<Subscription> EnsureValidSubscriptionExist(SubscriptionData subscriptionData, CancellationToken cancellationToken = default);
        Task DeleteSubscription(string resource, string notificationUrl, CancellationToken cancellationToken = default);
        Task<Subscription> CreateSubscription(SubscriptionData subscriptionData, CancellationToken cancellationToken);
        Task<Subscription> RenewSubscription(string resource, string notificationUrl,
            int expirationOffsetDays = 2, CancellationToken cancellationToken = default);
    }

    public class ChangeNotificationSubscriptionManager : IChangeNotificationSubscriptionManager
    {
        protected readonly IGraphApiManager _graphApiManager;
        protected readonly ILogger<ChangeNotificationSubscriptionManager> _logger;

        public ChangeNotificationSubscriptionManager(
            IGraphApiManager graphApiManager,
            ILogger<ChangeNotificationSubscriptionManager> logger)
        {
            _graphApiManager = graphApiManager;
            _logger = logger;
        }

        public async Task<Subscription> Retrieve(string resource, string notificationUrl, CancellationToken cancellationToken = default)
        {
            var subscriptions = await _graphApiManager.GetSubscriptions(cancellationToken);
            var subscription = subscriptions.FirstOrDefault(a
                => a.Resource == resource && a.NotificationUrl == notificationUrl);
            return subscription;
        }

        public async Task<Subscription> EnsureValidSubscriptionExist(SubscriptionData subscriptionData,
            CancellationToken cancellationToken = default)
        {
            Subscription subscription = await Retrieve(subscriptionData.Resource, subscriptionData.NotificationUrl, cancellationToken);
            if (subscription == null)
            {
                _logger.LogInformation("No subscription found, creating new");
                subscription = await _graphApiManager.AddSubscription(
                            subscriptionData.ChangeType,
                            subscriptionData.Resource,
                            subscriptionData.NewExpiration,
                            subscriptionData.ClientStateSecret,
                            subscriptionData.NotificationUrl,
                            cancellationToken);
                
                _logger.LogInformation("Subscription created");
                return subscription;
            }

            subscription = await _graphApiManager.RenewSubscription(subscription.Id, subscriptionData.NewExpiration, cancellationToken);
            _logger.LogInformation("Subscription renewed");
            return subscription;
        }

        public async Task DeleteSubscription(string resource, string notificationUrl, CancellationToken cancellationToken = default)
        {
            var subscription = await _graphApiManager.GetSubscriptions(cancellationToken);
            var currentSubscription = subscription.FirstOrDefault(a =>
                a.Resource == resource && a.NotificationUrl == notificationUrl);

            if (currentSubscription == null)
            {
                throw new ArgumentException($"No subscription found");
            }
            await _graphApiManager.DeleteSubscription(currentSubscription.Id, cancellationToken);
        }

        public async Task<Subscription> CreateSubscription(SubscriptionData subscriptionData, CancellationToken cancellationToken)
        {
            var subscription = await _graphApiManager.AddSubscription(
                subscriptionData.ChangeType,
                subscriptionData.Resource,
                subscriptionData.NewExpiration,
                subscriptionData.ClientStateSecret,
                subscriptionData.NotificationUrl,
                cancellationToken);
            return subscription;
        }

        public async Task<Subscription> RenewSubscription(string resource, string notificationUrl,
            int expirationOffsetDays = 2, CancellationToken cancellationToken = default)
        {
            Subscription subscription = await Retrieve(resource, notificationUrl, cancellationToken);
            if (subscription == null)
                return null;

            subscription = await _graphApiManager.RenewSubscription(subscription.Id, expirationOffsetDays, cancellationToken);
            return subscription;
        }
    }
}