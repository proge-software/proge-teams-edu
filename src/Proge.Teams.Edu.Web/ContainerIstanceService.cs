using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.Web
{
    public interface IContainerIstanceService
    {
        Task StartContainerIstance(string ResourceGroupName, string ContainerGroupName, string functionName, int maxMinutesDuration = 180);
    }

    public class ContainerIstanceService : IContainerIstanceService
    {
        private readonly ILogger<ContainerIstanceService> _logger;
        public ContainerIstanceService(ILogger<ContainerIstanceService> logger)
        {
            _logger = logger;
        }

        public async Task StartContainerIstance(string ResourceGroupName, string ContainerGroupName, string functionName, int maxMinutesDuration = 180)
        {
            //first we will login using the credentials file                
            IAzure azure = Microsoft.Azure.Management.Fluent.Azure.Authenticate("credentials.release.json")
                .WithDefaultSubscription();
            //var currentSubscription = azure.GetCurrentSubscription();

            //then we retrieve the group of the container using the resource group name and the container group name
            var containerGroup = await azure.ContainerGroups.GetByResourceGroupAsync(ResourceGroupName, ContainerGroupName);
            if (containerGroup == null)
            {
                _logger.LogError($"Error at '{nameof(functionName)}': container group {containerGroup} does not exist in RG {ResourceGroupName}. The procedure does not support in line creation of container");
                return;
            }

            //here I set an expiration for the container, so if it's Running and if it started for more than 180 minutes, I'll stop it
            var container = containerGroup.Containers.FirstOrDefault().Value;

            //if it's not running, then it's in a pending state(starting or shutting down)
            if (containerGroup.State == "Running")
            {
                var startTime = container.InstanceView.CurrentState.StartTime.GetValueOrDefault();
                _logger.LogInformation($"Container started at {startTime}");

                if (DateTime.UtcNow.Subtract(startTime).TotalMinutes > maxMinutesDuration)
                {
                    _logger.LogWarning($"Container started at {startTime} and exceeded {maxMinutesDuration} duration");
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
                _logger.LogInformation($"Container started at {DateTime.Now} and exceeded duration");
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
