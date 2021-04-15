using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Proge.Teams.Edu.Web;

namespace Proge.Teams.Edu.Function
{
    public class StartTeamsProvisioningFunction
    {
        private readonly IContainerIstanceService _containerIstanceService;
        private readonly ILogger<ContainerIstanceService> _logger;
        public StartTeamsProvisioningFunction(IContainerIstanceService containerIstanceService, ILogger<ContainerIstanceService> logger)
        {
            _containerIstanceService = containerIstanceService;
            _logger = logger;
        }


        [FunctionName("StartTeamsProvisioningFunction")]
        public async Task Run([TimerTrigger("0 0 */8 * * *")] TimerInfo myTimer, ILogger log)
        {
            try
            {
                _logger.LogInformation($"C# Timer trigger '{nameof(StartTeamsProvisioningFunction)}' function executed at: {DateTime.Now}");
                await _containerIstanceService.StartContainerIstance("TeamsConnector", "iulm-teams-connect-ci", nameof(StartTeamsProvisioningFunction));
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error at '{nameof(StartTeamsProvisioningFunction)}'");
                Console.WriteLine(e);
            }
            finally
            {
                _logger.LogInformation($"C# Timer trigger '{nameof(StartTeamsProvisioningFunction)}' function terminated at: {DateTime.Now}");
            }
        }
    }
}
