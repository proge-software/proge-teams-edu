using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Proge.Teams.Edu.DAL;
using Proge.Teams.Edu.Web;
using System;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.Function
{
    public class TimeTriggeredFunction
    {

        private readonly ITeamsDashboardFunctionsService teamsDataCollectorManager;

        public TimeTriggeredFunction(ITeamsDashboardFunctionsService teamsDataCollector)
        {
            teamsDataCollectorManager = teamsDataCollector;
        }

        [FunctionName("TimeTriggeredFunction")]
        public async Task Run([TimerTrigger("0 0 4 * * *")] TimerInfo myTimer, ILogger log)
        {
            await teamsDataCollectorManager.RunTimeTriggeredFunction(nameof(TimeTriggeredFunction));
        }
    }
}
