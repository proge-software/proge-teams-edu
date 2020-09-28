using System;
using System.Linq;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Proge.Teams.Edu.ItalianUniversity.ScheduledFunction
{
    public static class StartTeamsProvisioningFunction
    {
        [FunctionName("StartTeamsProvisioningFunction")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            
            GetStatus("<ResourceGroupName>", "<ContainerIstanceName>");
        }

        public static void GetStatus(string ResourceGroupName, string ContainerGroupName)
        {
            try
            {
                //first we will login using the credentials file
                //IAzure azure = GetAzureContext(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));
                IAzure azure = GetAzureContext();
                //then we retrieve the group of the container using the resource group name and the container group name
                var containerGroup = azure.ContainerGroups.GetByResourceGroup(ResourceGroupName, ContainerGroupName);
                if (containerGroup == null)
                {
                    //this means the container doesn't exist, so we can create it
                    return;
                }

                //here I set an expiration for the container, so if it's Running and if it started for more than 10 minutes, I'll stop it
                var container = containerGroup.Containers.FirstOrDefault().Value;

                //if it's not running, then it's in a pending state(starting or shutting down)
                if (containerGroup.State == "Running")
                {
                    var startTime = container.InstanceView.CurrentState.StartTime.GetValueOrDefault();
                    //Logger.Log($"Container started at {startTime}");
                    if (DateTime.UtcNow.Subtract(startTime).TotalMinutes > 180)
                    {
                        //Logger.Log($"Container started at {startTime} and exceeded duration");                
                    }
                }

                containerGroup.Restart();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static IAzure GetAzureContext()
        {
            IAzure azure = Azure.Authenticate("credentials.release.json").WithDefaultSubscription();
            //var currentSubscription = azure.GetCurrentSubscription();
            return azure;
        }
    }
}
