using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.ContainerInstance.Fluent.ContainerGroup.Definition;
using Microsoft.Azure.Management.ContainerInstance.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Graph.RBAC.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Extensions.Logging;
using static Microsoft.Azure.Management.Fluent.Azure;

namespace Proge.Teams.Edu.Web
{
    /// <summary>
    /// Starts or restarts the Azure Container Instance for PATC
    /// </summary>
    public interface IContainerInstanceService
    {
        [Obsolete]
        Task StartContainerInstance(string ResourceGroupName, string ContainerGroupName, string functionName, int maxMinutesDuration = 180);
        Task StartContainerInstance(ServicePrincipal sp, string ResourceGroupName, string ContainerGroupName, string functionName,
            string networkSubscriptionId, string networkResourceGroup, string networkProfileName,
            string azureRegion = "westeurope", string containerImage = "mcr.microsoft.com/azuredocs/aci-helloworld:latest",
            string crName = null, string crUser = null, string crPassword = null,
            Dictionary<string, string> environmentVariables = null,
            int maxMinutesDuration = 180, CancellationToken cancellationToken = default);
        Task StopContainerInstance(ServicePrincipal servicePrincipal, string resourceGroupName,
            string containerGroupName, CancellationToken cancellationToken = default);
    }


    public class ServicePrincipal
    {
        public string Client { get; set; }
        public string Key { get; set; }
        public string Tenant { get; set; }
        public string Subscription { get; set; }
    }

    public class ContainerInstanceService : IContainerInstanceService
    {
        private readonly ILogger<ContainerInstanceService> _logger;
        public ContainerInstanceService(ILogger<ContainerInstanceService> logger)
        {
            _logger = logger;
        }

        public async Task StartContainerInstance(ServicePrincipal servicePrincipal,
            string ResourceGroupName, string ContainerGroupName, string functionName,
            string networkSubscriptionId, string networkResourceGroup, string networkProfileName,
            string azureRegion = "westeurope", string containerImage = "mcr.microsoft.com/azuredocs/aci-helloworld:latest",
            string crName = null, string crUser = null, string crPassword = null,
            Dictionary<string, string> environmentVariables = null, int maxMinutesDuration = 180, CancellationToken cancellationToken = default)
        {
            (IAuthenticated authenticated, IAzure azure) = Authenticate(servicePrincipal);

            await UpdateAndStartContainerInstance(
                authenticated: authenticated,
                azure: azure,
                resourceGroupName: ResourceGroupName,
                containerGroupName: ContainerGroupName,
                networkSubscriptionId: networkSubscriptionId,
                networkResourceGroup: networkResourceGroup,
                networkProfileName: networkProfileName,
                azureRegion: azureRegion,
                containerImage: containerImage,
                crName: crName,
                crUser: crUser,
                crPassword: crPassword,
                environmentVariables: environmentVariables,
                maxMinutesDuration: maxMinutesDuration,
                cancellationToken: cancellationToken);
        }

        [Obsolete]
        public async Task StartContainerInstance(string ResourceGroupName, string ContainerGroupName, string functionName, int maxMinutesDuration = 180)
        {
            IAzure azure = Microsoft.Azure.Management.Fluent.Azure.Authenticate("credentials.release.json").WithDefaultSubscription();
            await StartContainerInstance(azure, ResourceGroupName, ContainerGroupName, maxMinutesDuration: maxMinutesDuration);
        }

        private async Task UpdateAndStartContainerInstance(IAuthenticated authenticated, IAzure azure, string resourceGroupName, string containerGroupName,
            string networkSubscriptionId, string networkResourceGroup, string networkProfileName,
            string azureRegion, string containerImage, string crName = null, string crUser = null, string crPassword = null,
            Dictionary<string, string> environmentVariables = null, int maxMinutesDuration = 180, CancellationToken cancellationToken = default)
        {
            await EnsureDeleted(azure, resourceGroupName, containerGroupName, cancellationToken);

            _logger.LogInformation("Recreating the Azure Container Instance with name {0} in resource group {1}",
                containerGroupName, resourceGroupName);
            await CreateContainerInstance(authenticated, azure, resourceGroupName, containerGroupName,
                networkSubscriptionId, networkResourceGroup, networkProfileName, azureRegion,
                containerImage, crName, crUser, crPassword,
                environmentVariables: environmentVariables, cancellationToken: cancellationToken);
        }

        private async Task EnsureDeleted(IAzure azure, string resourceGroupName, string containerGroupName, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving Azure Container Instance {0} in resource group {1}", containerGroupName, resourceGroupName);
            IContainerGroup containerGroup = await azure.ContainerGroups.GetByResourceGroupAsync(resourceGroupName, containerGroupName, cancellationToken);
            if (containerGroup == null)
            {
                _logger.LogInformation("Azure Container Instance {0} in resource group {1} not found", containerGroupName, resourceGroupName);
                return;
            }

            _logger.LogInformation("Deleting Azure Container Instance ({0}) with name {1} in resource group {2}",
                containerGroup.Id, containerGroup.Name, containerGroup.ResourceGroupName);
            await azure.ContainerGroups.DeleteByIdAsync(containerGroup.Id, cancellationToken);

            _logger.LogInformation("Waiting for Azure Container Instance deletion");
            await WaitForDeletion(azure, containerGroup, containerGroupName, cancellationToken: cancellationToken);
        }

        private async Task WaitForDeletion(IAzure azure, IContainerGroup containerGroup, string containerGroupName,
            int delay = 2000, int maxRetry = 10, CancellationToken cancellationToken = default)
        {
            string resourceGroupName = containerGroup.ResourceGroupName;
            short retry = 0;
            while (retry < maxRetry)
            {
                IContainerGroup check = await azure.ContainerGroups.GetByResourceGroupAsync(resourceGroupName, containerGroupName, cancellationToken);
                if (check == null)
                {
                    TimeSpan partWaitedTime = TimeSpan.FromSeconds(delay * retry);
                    _logger.LogDebug("Azure Container Instance ({0}) with name {1} in resource group {2} deleted in {3}",
                        containerGroup.Id, containerGroup.Name, containerGroup.ResourceGroupName, partWaitedTime);
                    return;
                }

                await Task.Delay(delay);
                retry++;
            }

            TimeSpan waitedTime = TimeSpan.FromSeconds(delay * maxRetry);
            _logger.LogWarning("Deletion of Azure Container Instance ({0}) with name {1} in resource group {2} took too long (more than {3}): stop waiting",
                    containerGroup.Id, containerGroup.Name, containerGroup.ResourceGroupName, waitedTime);
            throw new Exception(
                string.Format("Deletion of Azure Container Instance ({0}) with name {1} in resource group {2} took too long (more than {3})",
                containerGroup.Id, containerGroup.Name, containerGroup.ResourceGroupName, waitedTime));
        }

        private async Task<IContainerGroup> CreateContainerInstance(IAuthenticated authenticated, IAzure azure,
            string resourceGroupName, string containerGroupName,
            string networkSubscriptionId, string networkResourceGroup, string networkProfileName,
            string azureRegion, string containerImage,
            string crName = null, string crUser = null, string crPassword = null,
            int cpuCount = 2, int memorySizeGB = 4,
            Dictionary<string, string> environmentVariables = null, CancellationToken cancellationToken = default)
        {
            var cibuilder = azure.ContainerGroups.Define(containerGroupName)
                            .WithRegion(azureRegion)
                            .WithExistingResourceGroup(resourceGroupName)
                            .WithLinux();

            IWithPrivateImageRegistryOrVolume pvbuilder = null;
            if (string.IsNullOrWhiteSpace(crName))
            {
                _logger.LogInformation("Using public image registry only");
                pvbuilder = cibuilder.WithPublicImageRegistryOnly();
            }
            else
            {
                _logger.LogInformation("Using private registry: {0}, {1}, {2}", crName, crUser, crPassword);
                pvbuilder = cibuilder.WithPrivateImageRegistry(crName, crUser, crPassword);
            }

            IContainerGroup containerGroup = await pvbuilder.WithoutVolume()
                .DefineContainerInstance(containerGroupName + "-1")
                    .WithImage(containerImage)
                    .WithExternalTcpPort(8082) // dummy port for network integration
                    .WithEnvironmentVariables(environmentVariables)
                    .WithCpuCoreCount(cpuCount)
                    .WithMemorySizeInGB(memorySizeGB)
                    .Attach()
                .WithRestartPolicy(ContainerGroupRestartPolicy.Never)
                .WithNetworkProfileId(networkSubscriptionId, networkResourceGroup, networkProfileName)
                .CreateAsync(cancellationToken);

            return containerGroup;
        }

        public async Task StopContainerInstance(ServicePrincipal servicePrincipal,
            string resourceGroupName, string containerGroupName, CancellationToken cancellationToken = default)
        {
            (IAuthenticated authenticated, IAzure azure) = Authenticate(servicePrincipal);
            await StopContainerInstance(authenticated, azure, resourceGroupName, containerGroupName, cancellationToken);
        }

        private async Task StopContainerInstance(IAuthenticated authenticated, IAzure azure,
            string resourceGroupName, string containerGroupName, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving container group {0} in resource group {1}",
                containerGroupName, resourceGroupName);
            IContainerGroup containerGroup = await azure.ContainerGroups
                .GetByResourceGroupAsync(resourceGroupName, containerGroupName, cancellationToken);
            if (containerGroup == null)
            {
                _logger.LogInformation("An Azure Container Instance ({0}) with name {1} in resource group {2} does not exist",
                    containerGroup.Id, containerGroup.Name, containerGroup.ResourceGroupName);
                return;
            }

            _logger.LogInformation("Stopping Azure Contaienr Instance ({0}) with name {1} in resource group {2}",
                    containerGroup.Id, containerGroup.Name, containerGroup.ResourceGroupName);
            await containerGroup.StopAsync(cancellationToken);
        }

        private async Task RegisterRoleAssignment(IAuthenticated authenticated, IContainerGroup containerGroup, string contributorId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(contributorId))
            {
                _logger.LogInformation("Adding a Role Assignment as Contributor for user {0} and Azure Container Instance {1}", contributorId, containerGroup.Id);
                return;
            }

            _logger.LogInformation("Adding a Role Assignment as Contributor for user {0} and Azure Container Instance {1}", contributorId, containerGroup.Id);
            IActiveDirectoryUser user = await authenticated.ActiveDirectoryUsers.GetByIdAsync(contributorId, cancellationToken);
            _logger.LogInformation("Found user with Id {0}: {1}", user.Id, user.Name);
            IRoleAssignment roleAssignment = await authenticated.RoleAssignments
                .Define(SdkContext.RandomGuid())
                .ForUser(user)
                .WithBuiltInRole(BuiltInRole.Contributor)
                .WithResourceScope(containerGroup)
                .CreateAsync(cancellationToken);
            _logger.LogInformation("Role assignment for user {0} and ACI {1} created with Id {2}", user.Id, containerGroup.Id, roleAssignment.Id);
        }

        [Obsolete]
        private async Task StartContainerInstance(IAzure azure, string resourceGroupName, string containerGroupName,
            int maxMinutesDuration = 180, CancellationToken cancellationToken = default)
        {
            //then we retrieve the group of the container using the resource group name and the container group name
            IContainerGroup containerGroup = await azure.ContainerGroups.GetByResourceGroupAsync(resourceGroupName, containerGroupName);
            if (containerGroup == null)
            {
                _logger.LogError(
                    "Container group {1} does not exist in RG {2}. The procedure does not support in line creation of container",
                    containerGroup, resourceGroupName);
                throw new Exception(
                    $"Container group {containerGroup} does not exist in RG {resourceGroupName}. The procedure does not support in line creation of container");
            }

            Container container = containerGroup.Containers.FirstOrDefault().Value;
            //if it's not running, then it's in a pending state(starting or shutting down)
            if (containerGroup.State == "Running")
            {
                var startTime = container.InstanceView.CurrentState.StartTime.GetValueOrDefault();
                _logger.LogInformation("Container started at {0}", startTime);

                if (DateTime.UtcNow.Subtract(startTime).TotalMinutes > maxMinutesDuration)
                {
                    _logger.LogWarning("Container started at {0} and exceeded {1} duration", startTime, maxMinutesDuration);
                    if (container.InstanceView.CurrentState.State == "Terminated")
                    {
                        await containerGroup.Manager.Inner.ContainerGroups
                            .StartAsync(containerGroup.ResourceGroupName, containerGroup.Name)
                            ;
                    }
                    else
                    {
                        await containerGroup.RestartAsync();
                    }
                }
            }
            else
            {
                _logger.LogInformation("Container started at {0} and exceeded duration", DateTime.Now);
                if (container.InstanceView.CurrentState.State == "Terminated")
                {
                    await containerGroup.Manager.Inner.ContainerGroups.StartAsync(containerGroup.ResourceGroupName, containerGroup.Name);
                }
                else
                {
                    await containerGroup.RestartAsync();
                }
            }
        }

        private (IAuthenticated, IAzure) Authenticate(ServicePrincipal sp)
        {
            var credentials = new AzureCredentialsFactory()
                .FromServicePrincipal(sp.Client, sp.Key, sp.Tenant, AzureEnvironment.AzureGlobalCloud);
            IAuthenticated authenticated = Microsoft.Azure.Management.Fluent.Azure.Authenticate(credentials);
            IAzure azure = authenticated.WithSubscription(sp.Subscription);
            return (authenticated, azure);
        }
    }
}
