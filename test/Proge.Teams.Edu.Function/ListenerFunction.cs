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
    /// Endpoint la gestione delle notifiche attraverso WebHook da parte di Microsoft Graph per il servizio Microsoft Teams
    /// </summary>
    public class ListenerFunction
    {

        private readonly ITeamsDashboardFunctionsService teamsDataCollectorManager;

        public ListenerFunction(ITeamsDashboardFunctionsService teamsDataCollector)
        {
            teamsDataCollectorManager = teamsDataCollector;
        }

        // The notificationUrl endpoint that's registered with the webhook subscription.
        [FunctionName("ListenerFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            return await teamsDataCollectorManager.RunListenerFunction(nameof(ListenerFunction), req);
        }
    }
}
