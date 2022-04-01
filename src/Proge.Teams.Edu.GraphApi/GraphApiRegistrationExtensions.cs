using Proge.Teams.Edu.Abstraction;
using Proge.Teams.Edu.GraphApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class GraphApiRegistrationExtensions
    {
        public static IServiceCollection RegisterGraphApiServices(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddScoped<IGraphRetrier, GraphRetrier>()
                .AddScoped<IGraphApiManager, GraphApiManager>()
                .AddScoped<IBetaGraphApiManager, BetaGraphApiManager>();
        }

        public static IServiceCollection RegisterGraphApiTeamsServices(this IServiceCollection serviceCollection)
        {
            return serviceCollection.RegisterGraphApiServices()
                .AddScoped<ITeamsManager, TeamsManager>()
                ;
        }
    }
}
