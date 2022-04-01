using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Proge.Teams.Edu.TeamsDashboard;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.Web
{
    /// <summary>
    /// This class consumes the MS Graph notifications (HTTP Webhook) about Microsoft Teams 
    /// meeting update (created, deleted, updated, and so on...)
    /// </summary>
    /// <returns>
    /// 201 if the call details was saved
    /// 204 if the notification did not lead to the creation of any record
    /// </returns>
    public interface IGraphTeamsNotificationConsumer
    {
        Task<IActionResult> Run(HttpRequest req);
    }

    public class GraphTeamsNotificationConsumer : IGraphTeamsNotificationConsumer
    {
        private readonly ITeamsDataCollectorManager _teamsDataCollectorManager;
        private readonly ILogger<IGraphTeamsNotificationConsumer> _logger;

        public GraphTeamsNotificationConsumer(
            ITeamsDataCollectorManager teamsDataCollector,
            ILogger<IGraphTeamsNotificationConsumer> logger)
        {
            _teamsDataCollectorManager = teamsDataCollector;
            _logger = logger;
        }

        public async Task<IActionResult> Run(HttpRequest req)
        {
            string validationToken = req.Query["validationToken"];
            if (!string.IsNullOrEmpty(validationToken))
            {
                // Validate the new subscription by sending the token back to Microsoft Graph.
                // This response is required for each subscription.
                _logger.LogDebug("Validation token found: {0}", validationToken);
                return new OkObjectResult(WebUtility.HtmlEncode(validationToken));
            }

            try
            {
                bool acquired = await _teamsDataCollectorManager.ProcessNotification(req.Body);
                if (acquired)
                    return new StatusCodeResult((int)HttpStatusCode.Created);
                return new NoContentResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Parsing Notification");
                throw;
            }
        }
    }
}