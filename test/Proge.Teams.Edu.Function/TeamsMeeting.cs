using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Proge.Teams.Edu.Function;
using Proge.Teams.Edu.TeamsDashaborad;
using Proge.Teams.Edu.TeamsDashboard;
using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Threading.Tasks;


namespace Proge.Teams.Edu.Function
{
    public class TeamsMeeting
    {
        private readonly AzureADJwtBearerValidation _azureADJwtBearerValidation;
        private readonly ITeamsDataCollectorManager _teamsDataCollectorManager;

        public TeamsMeeting(AzureADJwtBearerValidation azureADJwtBearerValidation, ITeamsDataCollectorManager teamsDataCollector)
        {
            _azureADJwtBearerValidation = azureADJwtBearerValidation;
            _teamsDataCollectorManager = teamsDataCollector;
        }

        [FunctionName("TeamsMeeting")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("C# HTTP trigger RandomStringAuthLevelAnonymous processed a request.");
                
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