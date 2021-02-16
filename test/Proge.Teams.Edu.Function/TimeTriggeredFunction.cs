using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Proge.Teams.Edu.DAL;
using Proge.Teams.Edu.TeamsDashaborad;
using System;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.Function
{
    public class TimeTriggeredFunction
    {

        private readonly TeamsEduDbContext teamsEduDbContext;
        private readonly ITeamsDataCollectorManager teamsDataCollectorManager;

        public TimeTriggeredFunction(TeamsEduDbContext db, ITeamsDataCollectorManager teamsDataCollector)
        {
            teamsDataCollectorManager = teamsDataCollector;
            teamsEduDbContext = db;
        }

        [FunctionName("TimeTriggeredFunction")]
        public async Task Run([TimerTrigger("0 0 4 * * *")] TimerInfo myTimer, ILogger log)
        {
            try
            {
                log.LogInformation($"C# Timer trigger '{nameof(TimeTriggeredFunction)}' function executed at: {DateTime.Now}");
                teamsEduDbContext.EnsureMigrationOfContext();
                await teamsDataCollectorManager.SubscribeCallRecords();
            }
            catch (Exception e)
            {
                log.LogError(e, $"Error at '{nameof(TimeTriggeredFunction)}'");
                Console.WriteLine(e);
            }
            finally
            {
                log.LogInformation($"C# Timer trigger '{nameof(TimeTriggeredFunction)}' function terminated at: {DateTime.Now}");
            }
        }
    }
}
