using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Proge.Teams.Edu.DAL
{
    public abstract class DesignTimeDbContextFactoryBase<TContext> : IDesignTimeDbContextFactory<TContext> where TContext : DbContext
    {
        public TContext CreateDbContext(string[] args)
        {
            //Uncomment these lines, if you wish to debug Entity Framework Core command line operation
            //if (!System.Diagnostics.Debugger.IsAttached)
            //    System.Diagnostics.Debugger.Launch();
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var migrationAssembly = Environment.GetEnvironmentVariable("MIGRATIONS_ASSEMBLY");
            return Create(Directory.GetCurrentDirectory(), environmentName, migrationAssembly);
        }
        protected abstract TContext CreateNewInstance(DbContextOptions<TContext> options);

        public TContext Create()
        {
            //Uncomment these lines, if you wish to debug Entity Framework Core command line operation
            //if (!System.Diagnostics.Debugger.IsAttached)
            //    System.Diagnostics.Debugger.Launch();
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var migrationAssembly = Environment.GetEnvironmentVariable("MIGRATIONS_ASSEMBLY");
            var basePath = AppContext.BaseDirectory;
            return Create(basePath, environmentName, migrationAssembly);
        }

        private TContext Create(string basePath, string environmentName, string migrationAssembly = "Proge.Teams.Edu.DAL")
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json", true)
                .AddJsonFile($"appsettings.local.json", true)
                .AddJsonFile($"appsettings.release.json", true)
                ;

            var config = builder.Build();
            var connstr = config.GetConnectionString("DefaultConnection");

            if (String.IsNullOrWhiteSpace(connstr))
            {
                throw new InvalidOperationException("Could not find a connection string named 'default'.");
            }
            else
            {
                return Create(connstr);
            }
        }

        private TContext Create(string connectionString, string migrationAssembly = "Proge.Teams.Edu.DAL")
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException($"{nameof(connectionString)} is null or empty.", nameof(connectionString));
            }

            var optionsBuilder = new DbContextOptionsBuilder<TContext>();            
            System.Console.WriteLine("MyDesignTimeDbContextFactory.Create(string): Connection string: {0}", connectionString);
            optionsBuilder.UseSqlServer(connectionString, opt => {
                opt.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds);
                opt.MigrationsAssembly(migrationAssembly);
            });
            DbContextOptions<TContext> options = optionsBuilder.Options;
            return CreateNewInstance(options);
        }
    }
}
