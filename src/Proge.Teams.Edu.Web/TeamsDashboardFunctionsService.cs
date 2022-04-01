using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Proge.Teams.Edu.DAL;
using Proge.Teams.Edu.TeamsDashboard;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.Web
{
    [Obsolete]
    public interface ITeamsDashboardFunctionsService
    {
        [Obsolete("Use Proge.Teams.Edu.Web.GraphTeamsNotificationConsumer instead")]
        Task<IActionResult> RunListenerFunction(string functionNameHttpRequest, HttpRequest req);
        [Obsolete("Use Proge.Teams.Edu.Web.TeamsMeetingEnricher instead")]
        Task<IActionResult> RunTeamsMeeting(HttpRequest req);
        [Obsolete("Use Proge.Teams.Edu.Web.GraphApiSubscriptionRenewer instead")]
        Task RunTimeTriggeredFunction(string functionName);
    }

    [Obsolete]
    public class TeamsDashboardFunctionsService : ITeamsDashboardFunctionsService
    {
        private readonly IAzureADJwtBearerValidation _azureADJwtBearerValidation;
        private readonly ITeamsDataCollectorManager _teamsDataCollectorManager;
        private readonly ILogger<TeamsDashboardFunctionsService> _logger;
        private readonly TeamsEduDbContext _teamsEduDbContext;

        public TeamsDashboardFunctionsService(IAzureADJwtBearerValidation azureADJwtBearerValidation,
            ITeamsDataCollectorManager teamsDataCollector,
            ILogger<TeamsDashboardFunctionsService> logger,
            TeamsEduDbContext teamsEduDbContext)
        {
            _azureADJwtBearerValidation = azureADJwtBearerValidation;
            _teamsDataCollectorManager = teamsDataCollector;
            _logger = logger;
            _teamsEduDbContext = teamsEduDbContext;
        }

        [Obsolete("Use Proge.Teams.Edu.Web.TeamsMeetingEnricher instead")]
        public async Task<IActionResult> RunTeamsMeeting(HttpRequest req)
        {
            try
            {
                _logger.LogInformation("C# HTTP trigger RandomStringAuthLevelAnonymous processed a request.");

                ClaimsPrincipal principal; // This can be used for any claims
                if ((principal = await _azureADJwtBearerValidation.ValidateTokenAsync(req.Headers["Authorization"])) == null)
                {
                    return ActionResult(StatusCodes.Status401Unauthorized);
                }

                //var claimsName = $"Bearer token claim preferred_username: {_azureADJwtBearerValidation.GetPreferredUserName()}";
                //return new OkObjectResult($"{claimsName} {GetEncodedRandomString()}");

                if (req.QueryString.HasValue && !string.IsNullOrWhiteSpace(req.Query["uniSenderKey"]))
                {
                    var writeResponse = await _teamsDataCollectorManager.WriteTeamsMeetingTable(req.Body, req.Query["uniSenderKey"]);

                    if (writeResponse.IsSuccess)
                    {
                        if (writeResponse.RetMessage == "update")
                        {
                            return ActionResult(StatusCodes.Status200OK);
                        }
                        return ActionResult(StatusCodes.Status201Created);
                    }
                    else
                    {
                        return ActionResult(StatusCodes.Status400BadRequest, writeResponse.RetMessage);
                    }
                }
                else
                {
                    return ActionResult(StatusCodes.Status400BadRequest, "Missing query-string parameter.");
                }

            }
            catch (Exception ex)
            {
                if (ex is SecurityTokenExpiredException)
                    return ActionResult(StatusCodes.Status401Unauthorized, ex.Message);
                else
                    return ActionResult(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Obsolete("Use Proge.Teams.Edu.Web.GraphApiSubscriptionRenewer instead and add EnsureMigrationOfContext in Startup")]
        public async Task RunTimeTriggeredFunction(string functionName)
        {
            try
            {
                _logger.LogInformation($"C# Timer trigger '{nameof(functionName)}' function executed at: {DateTime.Now}");
                _teamsEduDbContext.EnsureMigrationOfContext();
                await _teamsDataCollectorManager.SubscribeCallRecords();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error at '{nameof(functionName)}'");
                throw;
            }
            finally
            {
                _logger.LogInformation($"C# Timer trigger '{nameof(functionName)}' function terminated at: {DateTime.Now}");
            }
        }

        [Obsolete("Use Proge.Teams.Edu.Web.GraphTeamsNotificationConsumer instead")]
        public async Task<IActionResult> RunListenerFunction(string functionNameHttpRequest, HttpRequest req)
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
                await _teamsDataCollectorManager.ProcessNotification(req.Body);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ParsingNotification: { ex.Message }");
            }

            // Still return a 202 so the service doesn't resend the notification.
            return new AcceptedResult();
        }

        private static ActionResult ActionResult(int statusCode, string reason = "")
        {
            return new ContentResult
            {
                StatusCode = statusCode,
                Content = $"Status Code: {statusCode} ({ReasonPhrases.GetReasonPhrase(statusCode)}); {reason}",
                ContentType = "text/plain"
            };
        }
    }
}