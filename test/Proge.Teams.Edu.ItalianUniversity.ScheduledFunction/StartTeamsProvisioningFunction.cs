using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Proge.Teams.Edu.ItalianUniversity.ScheduledFunction
{
    public static class StartTeamsProvisioningFunction
    {
        [FunctionName("StartTeamsProvisioningFunction")]
        public static void Run([TimerTrigger("0 0 */3 * * *")] TimerInfo myTimer, ILogger log)
        {
            try
            {
                log.LogInformation($"C# Timer trigger '{nameof(StartTeamsProvisioningFunction)}' function executed at: {DateTime.Now}");
                Task.Run(async () => await GetStatus("SNS_ICT_Provisioning", "sns-teams-provisioning", log));
            }
            catch (Exception e)
            {
                log.LogError(e, $"Error at '{nameof(StartTeamsProvisioningFunction)}'");
                Console.WriteLine(e);
            }
            finally
            {
                log.LogInformation($"C# Timer trigger '{nameof(StartTeamsProvisioningFunction)}' function terminated at: {DateTime.Now}");
            }
        }


        public static async Task GetStatus(string ResourceGroupName, string ContainerGroupName, ILogger log)
        {

            //should read from cfg
            int maxMinutesDuration = 180;
            //first we will login using the credentials file                
            IAzure azure = Azure.Authenticate("credentials.release.json").WithDefaultSubscription();
            //var currentSubscription = azure.GetCurrentSubscription();

            //then we retrieve the group of the container using the resource group name and the container group name
            var containerGroup = await azure.ContainerGroups.GetByResourceGroupAsync(ResourceGroupName, ContainerGroupName);
            if (containerGroup == null)
            {
                log.LogError($"Error at '{nameof(StartTeamsProvisioningFunction)}': container group {containerGroup} does not exist in RG {ResourceGroupName}. The procedure does not support in line creation of container");
                return;
            }

            //here I set an expiration for the container, so if it's Running and if it started for more than 180 minutes, I'll stop it
            var container = containerGroup.Containers.FirstOrDefault().Value;

            //if it's not running, then it's in a pending state(starting or shutting down)
            if (containerGroup.State == "Running")
            {
                var startTime = container.InstanceView.CurrentState.StartTime.GetValueOrDefault();
                log.LogInformation($"Container started at {startTime}");

                if (DateTime.UtcNow.Subtract(startTime).TotalMinutes > maxMinutesDuration)
                {
                    log.LogWarning($"Container started at {startTime} and exceeded {maxMinutesDuration} duration");
                    if (container.InstanceView.CurrentState.State == "Terminated")
                    {
                        await containerGroup.Manager.Inner.ContainerGroups.StartAsync(containerGroup.ResourceGroupName, containerGroup.Name);
                    }
                    else
                        await containerGroup.RestartAsync();
                }
            }
            else
            {
                log.LogInformation($"Container started at {DateTime.Now} and exceeded duration");
                if (container.InstanceView.CurrentState.State == "Terminated")
                {
                    await containerGroup.Manager.Inner.ContainerGroups.StartAsync(containerGroup.ResourceGroupName, containerGroup.Name);
                }
                else
                    await containerGroup.RestartAsync();
            }
        }
    }
}
