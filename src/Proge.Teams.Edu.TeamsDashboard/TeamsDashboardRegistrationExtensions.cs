using Proge.Teams.Edu.TeamsDashboard;
using Proge.Teams.Edu.TeamsDashboard.ChangeNotificationSubscription;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TeamsDashboardRegistrationExtensions
    {
        public static IServiceCollection RegisterTeamsDashboardServices(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddScoped<ITeamsDataCollectorManager, TeamsDataCollectorManager>()
                .AddScoped<ITeamMeetingBulkInsertOrUpdateHandler, TeamMeetingBulkInsertOrUpdateHandler>()
                .AddScoped<IAzureADJwtBearerValidation, AzureADJwtBearerValidation>()
                .AddScoped<IChangeNotificationSubscriptionManager, ChangeNotificationSubscriptionManager>()
                .AddScoped<IEventHubListener, EventHubListener>();
        }
    }
}
