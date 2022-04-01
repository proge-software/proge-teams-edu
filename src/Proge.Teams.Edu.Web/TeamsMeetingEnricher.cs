using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Proge.Teams.Edu.TeamsDashboard;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.Web
{
    /// <summary>
    /// This class consumes the Customer's request to enrich data about known Teams' meeting
    /// </summary>
    public interface ITeamsMeetingEnricher
    {
        Task<IActionResult> Run(HttpRequest req);
    }

    public class TeamsMeetingEnricher : ITeamsMeetingEnricher
    {
        private const string QueryStringUniSenderKey = "uniSenderKey";

        private readonly IAzureADJwtBearerValidation _azureADJwtBearerValidation;
        private readonly ITeamsDataCollectorManager _teamsDataCollectorManager;
        private readonly ILogger<TeamsMeetingEnricher> _logger;

        public TeamsMeetingEnricher(
            IAzureADJwtBearerValidation azureADJwtBearerValidation,
            ITeamsDataCollectorManager teamsDataCollector,
            ILogger<TeamsMeetingEnricher> logger)
        {
            _azureADJwtBearerValidation = azureADJwtBearerValidation;
            _teamsDataCollectorManager = teamsDataCollector;
            _logger = logger;
        }

        public async Task<IActionResult> Run(HttpRequest req)
        {
            _logger.LogInformation("Processing request to enrich a Teams meeting's info");
            ContentResult r = await ProcessRequest(req);
            _logger.LogInformation("Request processed with result {0}", r.Content);
            return r;
        }

        private async Task<ContentResult> ProcessRequest(HttpRequest req)
        {
            try
            {
                ClaimsPrincipal principal = await _azureADJwtBearerValidation.ValidateTokenAsync(req.Headers["Authorization"]);
                if (principal == null)
                    return ActionResult(StatusCodes.Status401Unauthorized);

                if (!req.QueryString.HasValue || string.IsNullOrWhiteSpace(req.Query[QueryStringUniSenderKey]))
                    return ActionResult(StatusCodes.Status400BadRequest, $"Missing query-string parameter '{QueryStringUniSenderKey}'");

                var writeResponse = await _teamsDataCollectorManager.WriteTeamsMeetingTable(req.Body, req.Query[QueryStringUniSenderKey]);
                if (!writeResponse.IsSuccess)
                    return ActionResult(StatusCodes.Status400BadRequest, writeResponse.RetMessage);

                if (writeResponse.RetMessage == "update")
                    return ActionResult(StatusCodes.Status200OK);

                return ActionResult(StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request to enrich the teams meeting table");

                if (ex is SecurityTokenExpiredException)
                    return ActionResult(StatusCodes.Status401Unauthorized, ex.Message);

                return ActionResult(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        private static ContentResult ActionResult(int statusCode, string reason = "")
        {
            return new ContentResult
            {
                StatusCode = statusCode,
                Content = $"Status Code: {statusCode} ({ReasonPhrases.GetReasonPhrase(statusCode)}); {reason}".TrimEnd(),
                ContentType = "text/plain"
            };
        }
    }
}