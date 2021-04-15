using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Proge.Teams.Edu.Abstraction;
using Proge.Teams.Edu.DAL;
using Proge.Teams.Edu.DAL.Repositories;
using Proge.Teams.Edu.GraphApi;
using Proge.Teams.Edu.TeamsDashaborad;
using Proge.Teams.Edu.TeamsDashboard;
using Proge.Teams.Edu.Web;
using Serilog;
using Serilog.Extensions.Logging;
using System.IO;

[assembly: FunctionsStartup(typeof(Proge.Teams.Edu.Function.Startup))]
namespace Proge.Teams.Edu.Function
{

    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", true, true)
                   .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true) //load local settings
                   //.AddJsonFile("appsettings.release.json", optional: true, reloadOnChange: true) //load local settings
                   .AddEnvironmentVariables()
                   .Build();

            var serilogLogger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            var services = builder.Services;

            services.AddScoped<IGraphApiManager, GraphApiManager>();
            services.AddScoped<IBetaGraphApiManager, BetaGraphApiManager>();
            services.AddScoped<ITeamsDataCollectorManager, TeamsDataCollectorManager>();
            services.AddScoped<IAzureADJwtBearerValidation, AzureADJwtBearerValidation>();
            services.AddScoped<ITeamsDashboardFunctionsService, TeamsDashboardFunctionsService>();
            services.AddScoped<ICallRecordRepository, CallRecordRepository>();
            services.AddScoped<ITeamsMeetingRepository, TeamsMeetingRepository>();
            services.Configure<AuthenticationConfig>(configuration.GetSection("ApplicationAuthentication"));
            services.Configure<UniSettings>(configuration.GetSection("UniSettings"));
            services.Configure<CallFilters>(configuration.GetSection("CallFilters"));

            builder.Services.AddSingleton<ILoggerProvider>(sp =>
            {
                var functionDependencyContext = DependencyContext.Load(typeof(Startup).Assembly);

                var hostConfig = sp.GetRequiredService<IConfiguration>();
                var logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(hostConfig, sectionName: "AzureFunctionsJobHost:Serilog", dependencyContext: functionDependencyContext)
                    .CreateLogger();

                return new SerilogLoggerProvider(logger, dispose: true);
            });

            services.AddDbContext<TeamsEduDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    serverOptions => serverOptions.MigrationsAssembly(typeof(TeamsEduDbContext).Assembly.FullName));
            });

            //services.EnsureMigrationOfContext<TeamsEduDbContext>();
            services.AddLogging(lb => lb.AddSerilog(serilogLogger));

#if DEBUG
            // to delete when go to Production
            IdentityModelEventSource.ShowPII = true;
#endif

            //services.BuildServiceProvider(true);
        }
    }

}
