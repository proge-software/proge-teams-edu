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
using Proge.Teams.Edu.Web;
using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Threading.Tasks;


namespace Proge.Teams.Edu.Function
{
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