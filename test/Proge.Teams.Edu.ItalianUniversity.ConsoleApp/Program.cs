using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Proge.Teams.Edu.Abstraction;
using Proge.Teams.Edu.DAL;
using Proge.Teams.Edu.DAL.Repositories;
using Proge.Teams.Edu.GraphApi;
using Serilog;
using Serilog.Core;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.ItalianUniversity.ConsoleApp
{
    class Program
    {
        private static ServiceProvider _serviceProvider;

        static async Task Main(string[] args)
        {
            Logger serilogLogger = null;

            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", true, true)
                    .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true) //load local settings
                    //.AddJsonFile("appsettings.sns.json", optional: true, reloadOnChange: true) //load local settings
                    .AddEnvironmentVariables()
                    .Build();

                serilogLogger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .CreateLogger();

                serilogLogger.Information("Loading...");

                RegisterServices(configuration, serilogLogger);
                IServiceScope scope = _serviceProvider.CreateScope();

                await scope.ServiceProvider.GetRequiredService<IConsoleApplication>()
                        .Run(args);

            }
            catch (Exception ex)
            {
                if (serilogLogger != null)
                    serilogLogger.Fatal(ex, "Process terminated unexpectedly");
            }
            finally
            {
                if (serilogLogger != null)
                {
                    serilogLogger.Information("Finished.");
                    serilogLogger.Dispose();
                }
                DisposeServices();
            }
        }

        private static void RegisterServices(IConfigurationRoot configuration, Serilog.ILogger serilogLogger)
        {
            var services = new ServiceCollection();
            services.AddScoped<IFacade, LessonsTeamsCreationFacade>();
            services.AddScoped<IJob, LessonsTeamsCreationJob>();
            //services.AddScoped<ISnsClient, SnsClient>();
            services.AddScoped<IGraphApiManager, GraphApiManager>();
            services.AddScoped<IBetaGraphApiManager, BetaGraphApiManager>();
            services.AddScoped<IConsoleApplication, ConsoleApplication>();
            services.AddScoped<ITeamRepository, TeamRepository>();
            services.Configure<AuthenticationConfig>(configuration.GetSection("ApplicationAuthentication"));

            services.AddDbContext<TeamsEduDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    serverOptions => serverOptions.MigrationsAssembly(typeof(TeamsEduDbContext).Assembly.FullName));
            });

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddSerilog(serilogLogger, true);
            });

            _serviceProvider = services.BuildServiceProvider(true);
        }

        private static void DisposeServices()
        {
            if (_serviceProvider == null)
            {
                return;
            }
            if (_serviceProvider is IDisposable)
            {
                ((IDisposable)_serviceProvider).Dispose();
            }
        }

    }
}
