using Microsoft.Extensions.Logging;
using Proge.Teams.Edu.DAL;
using System;

namespace Proge.Teams.Edu.Web
{
    /// <summary>
    /// This class can be used to migrate the database, however it is preferred to use
    /// `services.EnsureMigrationOfContext<DB_CONTEXT>()` in Startup after services' registration
    /// </summary>
    public interface IContextMigrator
    {
        void Migrate();
    }

    public class ContextMigrator : IContextMigrator
    {
        private readonly ILogger<ContextMigrator> _logger;
        private readonly TeamsEduDbContext _teamsEduDbContext;

        public ContextMigrator(
            ILogger<ContextMigrator> logger,
            TeamsEduDbContext teamsEduDbContext)
        {
            _logger = logger;
            _teamsEduDbContext = teamsEduDbContext;
        }

        public void Migrate()
        {
            try
            {
                _logger.LogInformation("Ensuring db is up to date");
                _teamsEduDbContext.EnsureMigrationOfContext();
                _logger.LogInformation("Database updated (if needed) successfully");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error updating the database");
                throw;
            }
        }
    }
}
