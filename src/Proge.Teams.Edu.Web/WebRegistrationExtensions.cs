using Proge.Teams.Edu.Web;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WebRegistrationExtensions
    {
        public static IServiceCollection RegisterEduWebServices(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddScoped<IContainerInstanceService, ContainerInstanceService>()
                .AddScoped<IGraphApiSubscriptionManager, GraphApiSubscriptionManager>()
                .AddScoped<ITeamsMeetingEnricher, TeamsMeetingEnricher>()
                .AddScoped<IGraphTeamsNotificationConsumer, GraphTeamsNotificationConsumer>();
        }
    }
}
