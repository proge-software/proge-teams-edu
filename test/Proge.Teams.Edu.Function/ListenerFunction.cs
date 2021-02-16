using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Proge.Teams.Edu.TeamsDashaborad;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.Function
{
    public class ListenerFunction
    {

        private readonly ITeamsDataCollectorManager teamsDataCollectorManager;

        public ListenerFunction(ITeamsDataCollectorManager teamsDataCollector)
        {
            teamsDataCollectorManager = teamsDataCollector;
        }

        // The notificationUrl endpoint that's registered with the webhook subscription.
        [FunctionName("ListenerFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            string validationToken = req.Query["validationToken"];
            if (!string.IsNullOrEmpty(validationToken))
            {
                // Validate the new subscription by sending the token back to Microsoft Graph.
                // This response is required for each subscription.
                log.LogDebug("Validation token found: {0}", validationToken);
                return new OkObjectResult(WebUtility.HtmlEncode(validationToken));
            }

            try
            {
                await teamsDataCollectorManager.ProcessNotification(req.Body);
            }
            catch (Exception ex)
            {
                log.LogError($"ParsingNotification: { ex.Message }");
            }

            // Still return a 202 so the service doesn't resend the notification.
            return new AcceptedResult();
        }
    }
}
