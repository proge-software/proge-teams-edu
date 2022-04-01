using Proge.Teams.Edu.Esse3;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class Esse3RegistrationExtensions
    {
        public static IServiceCollection RegisterEsse3Services(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddScoped<IRetryManager, RetryManager>()
                .AddScoped<IEsse3Client, Esse3Client>();
        }    

    }
}
