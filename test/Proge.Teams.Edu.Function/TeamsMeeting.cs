using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Proge.Teams.Edu.Web;
using System.Threading.Tasks;


namespace Proge.Teams.Edu.Function
{
    /// <summary>
    /// Endpoint per l'inserimento di dati customizzati da parte del cliente
    /// </summary>
    public class TeamsMeeting
    {
        private readonly ITeamsDashboardFunctionsService teamsDataCollectorManager;

        public TeamsMeeting(ITeamsDashboardFunctionsService teamsDataCollector)
        {
            teamsDataCollectorManager = teamsDataCollector;
        }

        [FunctionName("TeamsMeeting")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await teamsDataCollectorManager.RunTeamsMeeting(req);
        }
    }
}