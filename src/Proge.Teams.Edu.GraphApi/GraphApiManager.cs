﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.CallRecords;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using Proge.Teams.Edu.Abstraction;
using Proge.Teams.Edu.GraphApi.Models;

namespace Proge.Teams.Edu.GraphApi;


public class GraphApiManager : IGraphApiManager
{
    private readonly AuthenticationConfig _authenticationConfig;
    private IConfidentialClientApplication app { get; set; }
    private IGraphRetrier _retrier { get; set; }
    private GraphServiceClient graphClient { get; set; }
    private readonly ILogger<GraphApiManager> _logger;
    private static readonly HttpClient httpClient = new HttpClient();
    private string _bearerToken { get; set; }

    private readonly static JsonSerializerOptions DefaultSerializerOption = new JsonSerializerOptions
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };


    public GraphApiManager(IOptions<AuthenticationConfig> authCfg,
        ILogger<GraphApiManager> logger,
        IGraphRetrier retrier)
    {
        _retrier = retrier;
        _logger = logger;
        _authenticationConfig = authCfg.Value;
        app = ConfidentialClientApplicationBuilder
            .Create(_authenticationConfig.ClientId)
            .WithAuthority(AzureCloudInstance.AzurePublic, _authenticationConfig.TenantId)
            .WithClientSecret(_authenticationConfig.ClientSecret)
            .Build();

        graphClient = new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
        {
            _logger.LogTrace("Requesting new access token to Microsoft Graph API");
            var authResult = await app
                .AcquireTokenForClient(new string[1] { "https://graph.microsoft.com/.default" })
                .ExecuteAsync();

            _logger.LogTrace("Add access token to request message");
            _bearerToken = authResult.AccessToken;
            requestMessage
                .Headers
                .Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
        }));
    }

    /// <summary>
    /// List all the groups in an organization, including but not limited to Microsoft 365 groups.
    /// </summary>
    /// <returns>Collection of Microsoft.Graph.Group objects.</returns>
    public async Task<IGraphServiceGroupsCollectionPage> ListGroups()
    {
        var groups = await graphClient.Groups
            .Request()
            .GetAsync();

        return groups;
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptions(CancellationToken cancellationToken = default)
    {
        var allSubscriptions = new List<Subscription>();
        var subscriptions = await _retrier.Do(async () => await graphClient.Subscriptions
            .Request()
            .GetAsync(cancellationToken), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));

        while (subscriptions.Count > 0)
        {
            allSubscriptions.AddRange(subscriptions);
            if (subscriptions.NextPageRequest == null)
            {
                break;
            }
            subscriptions = await _retrier.Do(async () => await subscriptions.NextPageRequest
                .GetAsync(cancellationToken), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
        }
        return allSubscriptions;
    }

    public async Task<bool> RenewSubscriptions()
    {
        var allSubscriptions = await GetSubscriptions();
        bool isRenewed = false;

        foreach (Subscription subscription in allSubscriptions)
        {
            await RenewSubscription(subscription.Id);
            isRenewed = true;
        }

        return isRenewed;
    }

    public async Task DeleteSubscription(string id, CancellationToken cancellationToken = default)
    {
        await graphClient.Subscriptions[id]
            .Request()
            .DeleteAsync(cancellationToken);
    }

    public async Task<Subscription> RenewSubscription(string id, int expirationOffsetDays = 2, CancellationToken cancellationToken = default)
    {
        DateTimeOffset expirationDateTime = new(DateTime.UtcNow.AddDays(expirationOffsetDays), TimeSpan.Zero);
        Subscription subscription = await graphClient.Subscriptions[id]
            .Request()
            .UpdateAsync(new Subscription { ExpirationDateTime = expirationDateTime }, cancellationToken);
        return subscription;
    }

    public async Task<Subscription> RenewSubscription(string id, DateTimeOffset expirationDateTime, CancellationToken cancellationToken = default)
    {
        Subscription subscription = await graphClient.Subscriptions[id]
            .Request()
            .UpdateAsync(new Subscription { ExpirationDateTime = expirationDateTime }, cancellationToken);
        return subscription;
    }

    public async Task<Subscription> AddSubscription(
        string changeType, string resource, DateTimeOffset? expirationOffset, string clientStateSecret,
        string notificationUrl, CancellationToken cancellationToken = default)
    {
        Subscription subscription = new()
        {
            ChangeType = changeType,
            Resource = resource,
            ExpirationDateTime = expirationOffset,
            ClientState = clientStateSecret,
            NotificationUrl = notificationUrl,
            LatestSupportedTlsVersion = "v1_3",
        };

        var res = await graphClient.Subscriptions
            .Request()
            .AddAsync(subscription, cancellationToken);

        return res;
    }

    /// <summary>
    /// Get some user's data (id, userPrincipalName, jobTitle, officeLocation, department, mail and assignedLicenses) by his principal name.
    /// </summary>
    /// <param name="mail">User's principal name.</param>
    /// <returns>A Microsoft.Graph.User object.</returns>
    public async Task<User> GetUserIdByPrincipalName(string mail)
    {
        string filter = $"userPrincipalName eq '{mail}'";

        var users = await graphClient.Users
            .Request()
            .Filter(filter)
            .Select("id,userPrincipalName,jobTitle,officeLocation,department,mail,assignedLicenses")
            .GetAsync();

        // Check assignedLiceses as A1+ only have Teams

        return users.FirstOrDefault();
    }

    /// <summary>
    /// Get some user's data (userPrincipalName, mail and Azure AD id) by his principal name.
    /// </summary>
    /// <param name="userPrincipalName">User's principal name.</param>
    /// <returns>A Proge.Teams.Edu.GraphApi.Models.TeamMember object.</returns>
    public async Task<ITeamMember> GetTeamMemberByPrincipalName(string userPrincipalName)
    {
        string filter = $"userPrincipalName eq '{userPrincipalName}'";

        try
        {
            var users = await graphClient.Users
                   .Request()
                   .Filter(filter)
                   .Select("id,userPrincipalName,givenName,surname,mail")//,jobTitle,officeLocation,department,mail,assignedLicenses")
                   .GetAsync();

            // Check assignedLiceses as A1+ only have Teams
            var user = users.FirstOrDefault();
            if (user == null)
            {
                _logger.LogWarning($"User {userPrincipalName} not found in Azure AD");
                return null;
            }
            else
                return new TeamMember
                {
                    UserPrincipalName = user.UserPrincipalName,
                    Mail = user.Mail,
                    AzureAdId = user.Id,
                    Name = user.GivenName,
                    SecondName = user.Surname
                };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"User {userPrincipalName} not found in Azure AD");
            return null;
        }
    }

    /// <summary>
    /// Add a collection of users to the group's owners.
    /// </summary>
    /// <param name="groupid">Id of the group.</param>
    /// <param name="ownerIds">Collection of the users' ids.</param>
    /// <param name="ownersInGroup">Collection of Microsoft.Graph.DirectoryObject users (each with only id valued) that are already owner of the group. Optional.</param>
    /// <returns></returns>
    public async Task AddGroupOwners(string groupid, IEnumerable<string> ownerIds, IEnumerable<DirectoryObject> ownersInGroup = null)
    {
        foreach (var item in ownerIds.Distinct())
        {
            await AddGroupOwner(groupid, item, ownersInGroup);
        }
    }

    /// <summary>
    /// Add a user to the group's owners.
    /// </summary>
    /// <param name="groupid">Id of the group.</param>
    /// <param name="id">Id of the user.</param>
    /// <param name="ownersInGroup">Collection of Microsoft.Graph.DirectoryObject users (each with only id valued) that are already owner of the group. Optional.</param>
    /// <returns></returns>
    public async Task AddGroupOwner(string groupid, string id, IEnumerable<DirectoryObject> ownersInGroup = null)
    {
        if (ownersInGroup != null && ownersInGroup.Any(a => a.Id == id))
            return;

        var directoryObject = new DirectoryObject { Id = id };

        try
        {
            await _retrier.Do(async () => await graphClient.Groups[$"{groupid}"].Owners.References
                   .Request()
                   .AddAsync(directoryObject), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"AddGroupOwner: user {id} for group {groupid}");
        }
    }

    /// <summary>
    /// Add a collection of users to the group's members.
    /// </summary>
    /// <param name="groupid">Id of the group.</param>
    /// <param name="memeberIds">Collection of the users' ids.</param>
    /// <param name="membersInGroup">Collection of Microsoft.Graph.DirectoryObject users (each with only id valued) that are already members of the group. Optional.</param>
    /// <returns></returns>
    public async Task AddGroupMembers(string groupid, IEnumerable<string> memeberIds, IEnumerable<DirectoryObject> membersInGroup = null)
    {
        foreach (var item in memeberIds.Distinct())
        {
            await AddGroupMember(groupid, item, membersInGroup);
        }
    }

    /// <summary>
    /// Add a user to the group's members.
    /// </summary>
    /// <param name="groupid">Id of the group.</param>
    /// <param name="id">Id of the user.</param>
    /// <param name="membersInGroup">Collection of Microsoft.Graph.DirectoryObject users (each with only id valued) that are already members of the group. Optional.</param>
    /// <returns></returns>
    public async Task AddGroupMember(string groupid, string id, IEnumerable<DirectoryObject> membersInGroup = null)
    {
        if (membersInGroup != null && membersInGroup.Any(a => a.Id == id))
            return;

        var directoryObject = new DirectoryObject { Id = id };
        try
        {
            await _retrier.Do(async () => await graphClient.Groups[$"{groupid}"].Members.References
                   .Request()
                   .AddAsync(directoryObject), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"AddGroupMember: user {id} for group {groupid}");
        }
    }

    /// <summary>
    /// Remove a member from a group.
    /// </summary>
    /// <param name="groupId">Id of the group.</param>
    /// <param name="azureUserId">Azure id of the user to remove.</param>
    /// <returns></returns>
    public async Task RemoveGroupMember(string groupId, string azureUserId)
    {
        await graphClient.Groups[$"{groupId}"].Members[$"{azureUserId}"].Reference
            .Request()
            .DeleteAsync();
    }

    /// <summary>
    /// Remove an owner from a Microsoft 365 group, a security group, or a mail-enabled security group.
    /// </summary>
    /// <param name="groupId">Id of the group.</param>
    /// <param name="azureUserId">Azure id of the user to remove.</param>
    /// <returns></returns>
    public async Task RemoveGroupOwner(string groupId, string azureUserId)
    {
        await graphClient.Groups[$"{groupId}"].Owners[$"{azureUserId}"].Reference
            .Request()
            .DeleteAsync();
    }

    /// <summary>
    /// Replace the owners of a group.
    /// </summary>
    /// <param name="groupid">Id of the group.</param>
    /// <param name="newOwners">Collection of Proge.Teams.Edu.Abstraction.ITeamMember users (each with AzureAdId valued only) that replaces the existing owners list of the group.</param>
    /// <param name="existingOwners">Currently existing owners list of the group (collection of Microsoft.Graph.DirectoryObject, each with id valued only).</param>
    /// <returns></returns>
    public async Task UpdateGroupOwners(string groupid, IEnumerable<ITeamMember> newOwners, IEnumerable<DirectoryObject> existingOwners)
    {
        var newIds = newOwners == null || !newOwners.Any() ? Enumerable.Empty<string>() : newOwners.Select(a => a.AzureAdId);
        var existingIds = existingOwners == null || !existingOwners.Any() ? Enumerable.Empty<string>() : existingOwners.Select(a => a.Id);

        var removedOwners = existingIds.Except(newIds);
        var addedOwners = newIds.Except(existingIds);

        foreach (var id in removedOwners)
        {
            await RemoveGroupOwner(groupid, id);
        }

        foreach (var id in addedOwners)
        {
            await AddGroupOwner(groupid, id);
        }
    }

    /// <summary>
    /// Replace the members of a group.
    /// </summary>
    /// <param name="groupid">Id of the group.</param>
    /// <param name="newMembers">Collection of Proge.Teams.Edu.Abstraction.ITeamMember users (each with AzureAdId valued only) that replaces the existing members list of the group.</param>
    /// <param name="existingMembers">Currently existing members list of the group (collection of Microsoft.Graph.DirectoryObject, each with id valued only).</param>
    /// <returns></returns>
    public async Task UpdateGroupMembers(string groupid, IEnumerable<ITeamMember> newMembers, IEnumerable<DirectoryObject> existingMembers)
    {
        var newIds = newMembers == null || !newMembers.Any() ? Enumerable.Empty<string>() : newMembers.Select(a => a.AzureAdId);
        var existingIds = existingMembers == null || !existingMembers.Any() ? Enumerable.Empty<string>() : existingMembers.Select(a => a.Id);

        var removedMembers = existingIds.Except(newIds);
        var addedMembers = newIds.Except(existingIds);

        foreach (var id in removedMembers)
        {
            await RemoveGroupMember(groupid, id);
        }

        foreach (var id in addedMembers)
        {
            await AddGroupMember(groupid, id);
        }
    }

    /// <summary>
    /// Create a new team under a group (the group must have a least one owner).
    /// </summary>
    /// <param name="groupId">Id of the group.</param>
    /// <param name="team">A Microsoft.Graph.Team object that represent the team to create.</param>
    /// <returns>The new team (Microsoft.Graph.Team object).</returns>
    public async Task<Team> CreateTeam(string groupId, Team team)
    {
        var resTeam = await graphClient.Groups[$"{groupId}"].Team
            .Request()
            .PutAsync(team);
        return resTeam;
    }

    /// <summary>
    /// Update the properties of the specified team.
    /// </summary>
    /// <param name="groupId">Id of the group.</param>
    /// <param name="team">A Microsoft.Graph.Team object with the new values for relevant fields that should be updated.</param>
    /// <returns></returns>
    public async Task<Team> UpdateTeam(string groupId, Team team)
    {
        var resTeam = await graphClient.Groups[$"{groupId}"].Team
            .Request()
            .UpdateAsync(team);
        return resTeam;
    }

    /// <summary>
    /// Create a new channel in the team of a group.
    /// </summary>
    /// <param name="groupId">Id of the group.</param>
    /// <param name="channel">The Microsoft.Graph.Channel object to create.</param>
    /// <returns>The new channel (Microsoft.Graph.Channel object).</returns>
    public async Task<Channel> CreateChannel(string groupId, Channel channel)
    {
        var resChannel = await graphClient.Groups[$"{groupId}"].Team.Channels
            .Request()
            .AddAsync(channel);
        return resChannel;
    }

    /// <summary>
    /// Update a channel of the team of a group.
    /// </summary>
    /// <param name="groupId">Id of the group.</param>
    /// <param name="source">The channel (Microsoft.Graph.Channel object with id valued only) to update.</param>
    /// <param name="update">A Microsoft.Graph.Channel object with the new values for relevant fields that should be updated.</param>
    /// <returns></returns>
    public async Task<Channel> UpdateChannel(string groupId, Channel source, Channel update)
    {
        if (source.DisplayName == update.DisplayName)
            return source;

        var resChannel = await graphClient.Groups[$"{groupId}"].Team.Channels[$"{source.Id}"]
            .Request()
            .UpdateAsync(update);
        return resChannel;
    }

    /// <summary>
    /// Delete a channel of the team of a group.
    /// </summary>
    /// <param name="groupId">Id of the group.</param>
    /// <param name="chanId">Id of the channel to delete.</param>
    /// <returns></returns>
    public async Task DeleteChannel(string groupId, string chanId)
    {
        await graphClient.Groups[$"{groupId}"].Team.Channels[$"{chanId}"]
            .Request()
            .DeleteAsync();
    }

    /// <summary>
    /// Create a new group.
    /// </summary>
    /// <param name="group">The Microsoft.Graph.Group object to create.</param>
    /// <returns>The new group created (Microsoft.Graph.Group object).</returns>
    public async Task<Group> CreateGroup(Group group)
    {
        var resGroup = await graphClient.Groups
            .Request()
            .AddAsync(group);
        return resGroup;
    }

    /// <summary>
    /// Create a new education class.
    /// </summary>
    /// <param name="group">The Microsoft.Graph.EducationClass object to create.</param>
    /// <returns>The new Microsoft.Graph.EducationClass object created.</returns>
    public async Task<EducationClass> CreateEducationClass(EducationClass group)
    {
        return await _retrier.Do(async () => await graphClient.Education.Classes
            .Request()
            .AddAsync(group), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
    }

    /// <summary>
    /// Create a new team.
    /// </summary>
    /// <param name="team">The Microsoft.Graph.Team object to create.</param>
    /// <returns>The Microsoft.Graph.Team object created.</returns>
    public async Task<Team> CreateTeam(Team team)
    {
        return await _retrier.Do(async () => await graphClient.Teams
            .Request()
            .AddAsync(team), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
    }

    /// <summary>
    /// Adds (pins) a tab to the specified channel within a team.
    /// </summary>
    /// <param name="teamId">Id of the team.</param>
    /// <param name="chanId">Id of the channel.</param>
    /// <param name="tab">The Microsoft.Graph.TeamsTab to add.</param>
    /// <returns>The new Microsoft.Graph.TeamsTab object added to the channel.</returns>
    public async Task<TeamsTab> CreateWebTab(string teamId, string chanId, TeamsTab tab, CancellationToken cancellationToken = default)
    {
        return await _retrier.Do(async () => await graphClient.Teams[$"{teamId}"]
            .Channels[$"{chanId}"]
            .Tabs
           .Request()
           .AddAsync(tab, cancellationToken), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
    }

    /// <summary>
    /// Update the properties of the specified tab.
    /// </summary>
    /// <param name="groupId">Id of the team.</param>
    /// <param name="chanId">Id of the channel.</param>
    /// <param name="tabId">Id of the tab.</param>
    /// <param name="tab">A Microsoft.Graph.TeamsTab object with the new values for relevant fields that should be updated.</param>
    /// <returns></returns>
    public async Task<TeamsTab> UpdateWebTab(string groupId, string chanId, string tabId, TeamsTab tab, CancellationToken cancellationToken = default)
    {
        tab.ODataBind = null;
        return await _retrier.Do(async () => await graphClient.Teams[$"{groupId}"]
            .Channels[$"{chanId}"]
            .Tabs[$"{tabId}"]
            .Request()
            .UpdateAsync(tab, cancellationToken), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
    }

    /// <summary>
    /// Get a group by id.
    /// </summary>
    /// <param name="id">Id of the group.</param>
    /// <returns>The Microsoft.Graph.Group object.</returns>
    public async Task<Group> GetGroup(string id)
    {
        string filter = $"id eq '{id}'";
        var resGroup = await graphClient.Groups
            .Request()
            .Filter(filter)
            .GetAsync();
        return resGroup.FirstOrDefault();
    }

    /// <summary>
    /// Archive the specified team.
    /// </summary>
    /// <param name="id">Id of the team.</param>
    /// <returns></returns>
    public async Task ArchiveTeam(string id, bool? shouldSetSpoSiteReadOnlyForMembers = null)
    {
        try
        {
            await graphClient.Teams[$"{id}"]
                 .Archive(shouldSetSpoSiteReadOnlyForMembers)
                 .Request()
                 .PostAsync();
        }
        catch (ServiceException ex)
        {
            if (ex.StatusCode != System.Net.HttpStatusCode.NotFound)
                throw ex;
        }
    }


    /// <summary>
    /// UnArchive the specified team.
    /// </summary>
    /// <param name="id">Id of the team.</param>
    /// <returns></returns>
    public async Task UnArchiveTeam(string id)
    {
        try
        {
            await graphClient.Teams[$"{id}"]
                 .Unarchive()
                 .Request()
                 .PostAsync();
        }
        catch (ServiceException ex)
        {
            if (ex.StatusCode != System.Net.HttpStatusCode.NotFound)
                throw ex;
        }
    }

    /// <summary>
    /// Get a team by id.
    /// </summary>
    /// <param name="id">Id of the team.</param>
    /// <returns>The Microsoft.Graph.Team object.</returns>
    public async Task<Team> GetTeam(string id)
    {
        try
        {
            var resGroup = await _retrier.Do(async () => await graphClient.Teams[$"{id}"]
                  .Request()
                  .GetAsync(), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
            return resGroup;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Team {id} *probably* not found");
            return null;
        }
    }

    /// <summary>
    /// Get the members of a team.
    /// </summary>
    /// <param name="id">Id of the team.</param>
    /// <returns></returns>
    public async Task<IEnumerable<DirectoryObject>> GetTeamMembers(string id)
    {
        var allClasses = new List<DirectoryObject>();

        var classes = await _retrier.Do(async () => await graphClient.Groups[$"{id}"].Members
            .Request()
            .GetAsync(), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));

        while (classes.Count > 0)
        {
            allClasses.AddRange(classes);
            if (classes.NextPageRequest != null)
            {
                classes = await classes.NextPageRequest
                    .GetAsync();
            }
            else
            {
                break;
            }
        }
        return allClasses;
    }

    /// <summary>
    /// Get an education class with group and team.
    /// </summary>
    /// <param name="id">Id of the group/education class.</param>
    /// <returns>The Microsoft.Graph.EducationClass object.</returns>
    public async Task<EducationClass> GetEducationClassWGroupWTeam(string id)
    {
        var result = await _retrier.Do(async () => await GetEducationClass(id),
            TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));

        var group = await _retrier.Do(async () => await graphClient.Groups[id]
            .Request()
            .Expand("members")//,teams,owners")
            .GetAsync(),
            TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
        result.Group = group;

        try
        {
            result.Group.Team = await _retrier.Do(async () => await graphClient.Teams[id]
                    .Request()
                    .GetAsync(),
                    TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Team not found in group {id}");
        }

        var members = await GetTeamMembers(id);
        result.Group.Members.Clear();
        members.ToList().ForEach(a => result.Group.Members.Add(a));

        var owners = await _retrier.Do(async () => await graphClient.Groups[$"{id}"].Owners
            .Request()
            .GetAsync(),
            TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));

        result.Group.Owners = owners;

        return result;
    }

    /// <summary>
    /// Get a team with its channels and channels' tabs.
    /// </summary>
    /// <param name="id">Id of the team.</param>
    /// <returns>The Microsoft.Graph.Team object.</returns>
    public async Task<Team> GetTeamWChannelsWTabs(string id)
    {
        var result = await GetTeam(id);
        if (result == null)
            return result;

        var channels = await _retrier.Do(async () => await graphClient.Teams[$"{id}"].Channels
                .Request()
                .GetAsync(), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));

        result.Channels = channels;

        foreach (var item in channels)
        {
            var tabs = await _retrier.Do(async () => await graphClient.Teams[$"{id}"]
                .Channels[$"{item.Id}"]
                .Tabs
                .Request()
                .GetAsync(), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
            result.Channels.FirstOrDefault(a => a.Id == item.Id).Tabs = tabs;
        }

        return result;
    }

    /// <summary>
    /// Get the list of all class objects.
    /// </summary>
    /// <returns>The collection of Microsoft.Graph.EducationClass objects.</returns>
    public async Task<IEnumerable<EducationClass>> GetEducationClasses()
    {
        var allClasses = new List<EducationClass>();
        var classes = await graphClient.Education.Classes
            .Request()
            .GetAsync();

        while (classes.Count > 0)
        {
            allClasses.AddRange(classes);
            if (classes.NextPageRequest == null)
                break;

            classes = await classes.NextPageRequest.GetAsync();
        }

        return allClasses;
    }

    public async Task<EducationClass> GetEducationClassByMailNickname(string mailNickname)
    {
        var classes = await graphClient.Education.Classes
            .Request()
            .Filter($"$filter=mailNickname eq '{mailNickname}'")
            .GetAsync();

        return classes.FirstOrDefault();
    }

    /// <summary>
    /// Get a class.
    /// </summary>
    /// <param name="id">Id of the class.</param>
    /// <returns>The Microsoft.Graph.EducationClass object.</returns>
    public async Task<EducationClass> GetEducationClass(string id)
    {
        return await graphClient.Education.Classes[$"{id}"]
         .Request()
         .GetAsync();
    }

    /// <summary>
    /// Update the properties of a class.
    /// </summary>
    /// <param name="id">Id of the class.</param>
    /// <param name="educationClass">A Microsoft.Graph.EducationClass object with the new values for relevant fields that should be updated.</param>
    /// <returns>The updated Microsoft.Graph.EducationClass object.</returns>
    public async Task<EducationClass> UpdateEducationClass(string id, EducationClass educationClass)
    {
        return await graphClient.Education.Classes[$"{id}"]
           .Request()
           .UpdateAsync(educationClass);
    }

    /// <summary>
    /// Delete a class.
    /// </summary>
    /// <param name="id">Id of the class to delete.</param>
    /// <returns></returns>
    public async Task DeleteEducationClass(string id)
    {
        await graphClient.Education.Classes[$"{id}"]
         .Request()
         .DeleteAsync();
    }

    /// <summary>
    /// Retrieve a list of recently deleted groups.
    /// </summary>
    /// <param name="mailNickname">The SMTP address for the group.</param>
    /// <returns>collection of Microsoft.Graph.DirectoryObject.</returns>
    public async Task<IEnumerable<DirectoryObject>> GetRecentlyDeletedGroupsByMailNickname(string mailNickname)
    {
        try
        {
            string filter = $"mailNickname eq '{mailNickname}'";
            //var deletedGroups = await graphClient.Directory.DeletedItems
            //    .Request()
            //    .Filter(filter)
            //    .GetAsync();

            //var deletedGroups = await graphClient.Directory.DeletedItems["microsoft.graph.group"]
            //    .Request()
            //    //.Filter(filter)
            //    .GetAsync();

            //return deletedGroups;

            var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://graph.microsoft.com/v1.0/directory/deletedItems/microsoft.graph.group?$Filter={filter}")
            };

            if (string.IsNullOrWhiteSpace(_bearerToken))
                throw new System.Security.Authentication.AuthenticationException("Bearer token is empty");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);

            var response = await httpClient.SendAsync(requestMessage);

            string sResponse = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var dirObjCollection = JsonSerializer.Deserialize<IEnumerable<DirectoryObject>>(JObject.Parse(sResponse)["value"].ToString(), DefaultSerializerOption);

                return dirObjCollection;
            }
            else
            {
                throw new Exception($"{(int)response.StatusCode} {response.ReasonPhrase} {Environment.NewLine}{sResponse}");
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// Build a Microsoft.Graph.EducationClass object from a Proge.Teams.Edu.Abstraction.IEducationalClassTeam one.
    /// </summary>
    /// <param name="educationalClassTeam">The Proge.Teams.Edu.Abstraction.IEducationalClassTeam object.</param>
    /// <returns>The Microsoft.Graph.EducationClass object.</returns>
    public EducationClass DefaultEducationalClassFactory(IEducationalClassTeam educationalClassTeam)
    {
        if (educationalClassTeam.Key.Length > 64)
        {
            var msg = $"The team '{educationalClassTeam.Key}' MUST NOT exceed 64 characters";
            throw new Exception(msg);
        }

        return new EducationClass
        {
            MailNickname = educationalClassTeam.Key,
            //MS Graph limit on length
            DisplayName = educationalClassTeam.Name.Length >= 120 ? educationalClassTeam.Name.Substring(0, 120) : educationalClassTeam.Name,
            Description = educationalClassTeam.Description,
            //ClassCode = "Health 501",
            //ExternalName = "Health Level 1",
            ExternalId = educationalClassTeam.Key,
            ExternalSource = EducationExternalSource.Sis,
        };
    }

    /// <summary>
    /// Build a Microsoft.Graph.TeamsTab object.
    /// </summary>
    /// <param name="tabName">Name of the tab.</param>
    /// <param name="tabUrl">Web site url and content url of the tab.</param>
    /// <returns>The Microsoft.Graph.TeamsTab object.</returns>
    public TeamsTab DefaultWebTab(string tabName, string tabUrl)
    {
        return new TeamsTab()
        {
            ODataBind = $"https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/com.microsoft.teamspace.tab.web",
            DisplayName = tabName,
            Configuration = new TeamsTabConfiguration()
            {
                ODataType = null,
                EntityId = null,
                WebsiteUrl = tabUrl,
                ContentUrl = tabUrl,
                RemoveUrl = null
            }
        };
    }

    /// <summary>
    /// Build a Microsoft.Graph.Channel object.
    /// </summary>
    /// <param name="displayName">Display name of the channel.</param>
    /// <param name="description">Description of the channel.</param>
    /// <returns>The Microsoft.Graph.Channel object.</returns>
    public Channel DefaultChannelFactory(string displayName, string description = null)
    {
        return new Channel
        {
            DisplayName = displayName,
            Description = string.IsNullOrWhiteSpace(description) ? displayName : description
        };
    }

    /// <summary>
    /// Build a Microsoft.Graph.Team object for an existing group.
    /// </summary>
    /// <param name="groupId">An existing group id.</param>
    /// <returns>The Microsoft.Graph.Team object.</returns>
    public Team DefaultTeamFactory(string groupId)
    {
        return new Team
        {
            ODataType = null,
            AdditionalData = new Dictionary<string, object>()
                {
                    { "template@odata.bind", "https://graph.microsoft.com/beta/teamsTemplates('educationClass')" },
                    { "group@odata.bind", $"https://graph.microsoft.com/v1.0/groups('{groupId}')" }
                },
            GuestSettings = new TeamGuestSettings()
            {
                AllowCreateUpdateChannels = false,
                AllowDeleteChannels = false,
                ODataType = null,
            },
            MemberSettings = new TeamMemberSettings
            {
                ODataType = null,
                AllowDeleteChannels = false,
                AllowCreateUpdateChannels = false,
                AllowAddRemoveApps = false,
                AllowCreateUpdateRemoveTabs = false,
                AllowCreateUpdateRemoveConnectors = false,
            },
            MessagingSettings = new TeamMessagingSettings
            {
                ODataType = null,
                AllowUserEditMessages = false,
                AllowUserDeleteMessages = false,
                AllowOwnerDeleteMessages = true,
            },
            FunSettings = new TeamFunSettings
            {
                ODataType = null,
                AllowGiphy = true,
                GiphyContentRating = GiphyRatingType.Strict
            }
        };
    }

    /// <summary>
    /// GetUsersIdByPrincipalName: NOT WORKING
    /// </summary>
    /// <param name="mails"></param>
    /// <returns></returns>
    public async Task<IEnumerable<User>> GetUsersIdByPrincipalName(IEnumerable<string> mails)
    {
        if (mails == null || mails.Count() == 0)
            return Enumerable.Empty<User>();

        string query;
        if (mails.Count() == 1)
            query = $"'{mails.First()}'";
        else
            query = $"'{string.Join("\',", mails)}'";


        string filter = $"userPrincipalName in ({query})";

        var users = await graphClient.Users
            .Request()
            .Filter(filter)
            .Select("id,userPrincipalName,jobTitle,officeLocation,department,mail,assignedLicenses")
            .GetAsync();

        // Check assignedLiceses as A1+ only have Teams

        return users;
    }

    /// <summary>
    /// Archive and delete classes according to the starting value of the external id.
    /// </summary>
    /// <param name="externalIdPrefix">External id starting value of the classes to archive and delete.</param>
    /// <returns></returns>
    public async Task DeleteEducationalClassesByExternalIdPrefix(string externalIdPrefix)
    {
        var classes = await GetEducationClasses();
        var taskArchive = new List<Task>();
        var taskDelete = new List<Task>();
        foreach (var item in classes.Where(a => !string.IsNullOrWhiteSpace(a.ExternalId) && a.ExternalId.StartsWith(externalIdPrefix)))
        {
            taskArchive.Add(ArchiveTeam(item.Id));
            taskDelete.Add(DeleteEducationClass(item.Id));
        }
        await Task.WhenAll(taskArchive).ContinueWith(a => Task.WhenAll(taskDelete));
    }

    /// <summary>
    /// Get the collection of items in a Sharepoint list.
    /// </summary>
    /// <param name="siteId">Sharepoint site id.</param>
    /// <param name="listName">Sharepoint list name.</param>
    /// <returns>Collection of Sharepoint items (collection of Microsoft.Graph.ListItem).</returns>
    public async Task<IEnumerable<ListItem>> GetListItems(string siteId, string listName)
    {
        var list = await graphClient.Sites[siteId].Lists[listName].Items
            .Request()
            .GetAsync();
        return list;
    }

    public async Task<IEnumerable<ListItem>> GetListItemsWithFieldsValues(string siteId, string listId, IEnumerable<QueryOption> queryOptions = null, string filter = null)
    {
        List<ListItem> retList = new List<ListItem>();

        IListItemsCollectionRequest lstItCollectionRequest;
        IListItemsCollectionPage lstItCollectionPage;

        var queryBuilder = graphClient.Sites[siteId].Lists[listId].Items;
        if (queryOptions != null)
            lstItCollectionRequest = queryBuilder.Request(queryOptions);
        else
            lstItCollectionRequest = queryBuilder.Request();

        if (filter != null)
            lstItCollectionRequest = lstItCollectionRequest.Filter(filter);

        lstItCollectionPage = await lstItCollectionRequest.GetAsync();

        retList.AddRange(lstItCollectionPage);
        IListItemsCollectionRequest listItCollectionNextPageRequest = lstItCollectionPage.NextPageRequest;
        while (listItCollectionNextPageRequest != null)
        {
            IListItemsCollectionPage lstItCollectionNextPage = await listItCollectionNextPageRequest.GetAsync();
            listItCollectionNextPageRequest = lstItCollectionNextPage.NextPageRequest;
            retList.AddRange(lstItCollectionNextPage);
        }

        return retList;
    }


    /// <summary>
    /// Get a Sharepoint item.
    /// </summary>
    /// <param name="siteId">Sharepoint site id.</param>
    /// <param name="listName">Sharepoint list name.</param>
    /// <param name="itemId">Sharepoint item id.</param>
    /// <returns>Sharepoint item (Microsoft.Graph.ListItem).</returns>
    public async Task<ListItem> GetListItem(string siteId, string listName, string itemId)
    {
        var list = await graphClient.Sites[siteId].Lists[listName].Items[itemId]
            .Request()
            .GetAsync();
        return list;
    }

    /// <summary>
    /// Get a collection of Sharepoint items by an indexed field.
    /// </summary>
    /// <param name="siteId">Sharepoint site id.</param>
    /// <param name="listName">Sharepoint list name.</param>
    /// <param name="columnId">Id of the field.</param>
    /// <param name="value">Value of the field.</param>
    /// <returns>Collection of Sharepoint items (collection of Microsoft.Graph.ListItem).</returns>
    public async Task<IEnumerable<ListItem>> SearchListItemByIndexedField(string siteId, string listName, string columnId, string value)
    {
        string filter = $"fields/{columnId} eq '{value}'";
        var list = await graphClient.Sites[siteId].Lists[listName].Items
            .Request()
             .Filter(filter)
            .GetAsync();
        return list;
    }

    /// <summary>
    /// Get a collection of Sharepoint listitems including each listitem's column values.
    /// </summary>
    /// <param name="siteId">Sharepoint site id.</param>
    /// <param name="listName">Sharepoint list name.</param>
    /// <param name="columnId">Id of the search field.</param>
    /// <param name="value">Value of the search field.</param>
    /// <returns>Collection of Sharepoint items (collection of Microsoft.Graph.ListItem).</returns>
    public async Task<IEnumerable<ListItem>> SearchListItemDetailsByIndexedField(string siteId, string listName, string columnId, string value)
    {
        string filter = $"fields/{columnId} eq '{value}'";
        var queryOptions = new List<QueryOption>()
            {
                new QueryOption("expand", "fields")
            };
        var list = await graphClient.Sites[siteId].Lists[listName].Items
            .Request(queryOptions)
            .Filter(filter)
            .GetAsync();
        return list;
    }

    /// <summary>
    /// Add an item in a Sharepoint list.
    /// </summary>
    /// <param name="siteId">Sharepoint site id.</param>
    /// <param name="listName">Sharepoint list name.</param>
    /// <param name="item">Sharepoint item to add.</param>
    /// <returns>Sharepoint item (Microsoft.Graph.ListItem).</returns>
    public async Task<ListItem> AddListItem(string siteId, string listName, ListItem item)
    {
        var list = await graphClient.Sites[siteId].Lists[listName].Items
            .Request()
            .AddAsync(item);
        return list;
    }

    /// <summary>
    /// Update a Sharepoint item.
    /// </summary>
    /// <param name="siteId">Sharepoint site id.</param>
    /// <param name="listName">Sharepoint list name.</param>
    /// <param name="itemId">Sharepoint item id.</param>
    /// <param name="item">Column values specifying the fields to update.</param>
    /// <returns>Updated column values (Microsoft.Graph.FieldValueSet).</returns>
    public async Task<FieldValueSet> UpdateListItem(string siteId, string listName, string itemId, FieldValueSet item)
    {
        var list = await graphClient.Sites[siteId].Lists[listName].Items[itemId].Fields
            .Request()
            .UpdateAsync(item);
        return list;
    }

    /// <summary>
    /// Get a Sharepoint list.
    /// </summary>
    /// <param name="siteId">Sharepoint site id.</param>
    /// <param name="listName">Sharepoint list name.</param>
    /// <returns>The Sharepoint list (Microsoft.Graph.List).</returns>
    public async Task<List> GetListSite(string siteId, string listName)
    {
        var list = await graphClient.Sites[siteId].Lists[listName]
            .Request()
            .GetAsync();
        return list;
    }

    /// <summary>
    /// Get a Sharepoint site by uri (absolute path and dns safe host).
    /// </summary>
    /// <param name="uri">The site uri (with DnsSafeHost and AbsolutePath valued only).</param>
    /// <returns>The Sharepoint site (Microsoft.Graph.Site).</returns>
    public async Task<Site> GetSiteByUri(Uri uri)
    {
        var site = await graphClient.Sites.GetByPath(uri.DnsSafeHost, uri.AbsolutePath)
            .Request()
            .GetAsync();
        return site;
    }

    /// <summary>
    /// Get a Sharepoint site by keyword.
    /// </summary>
    /// <param name="key">Keyword to search for.</param>
    /// <returns>The Sharepoint site (Microsoft.Graph.Site).</returns>
    public async Task<IEnumerable<Site>> SearchSiteByKeywork(string key)
    {
        var queryOptions = new List<QueryOption>() { new QueryOption("search", key) };
        var site = await graphClient.Sites
            .Request(queryOptions)
            .GetAsync();
        return site;
    }

    /// <summary>
    /// Send an email.
    /// </summary>
    /// <param name="senderEmailAddress">E-mail address of the sender.</param>
    /// <param name="message">Microsoft.Graph.Message object that represent the message to send.</param>
    /// <returns></returns>
    public async Task SendMail(string senderEmailAddress, Message message, bool? SaveToSentItems = null)
    {
        try
        {
            await graphClient.Users[senderEmailAddress].SendMail(message).Request().PostAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Email not sent");
        }

    }

    public async Task<IEducationalClassTeam> MapTeamOwnerMemberPrincipalName(IEducationalClassTeam insegnamento, IDictionary<string, ITeamMember> userCache)
    {
        var taskOwners = new List<Task<ITeamMember>>();
        var taskMembers = new List<Task<ITeamMember>>();

        var cachedOwners = new List<ITeamMember>();
        foreach (var owner in insegnamento.Owners.Where(a => a != null && !string.IsNullOrWhiteSpace(a.UserPrincipalName)))
        {
            if (userCache.ContainsKey(owner.UserPrincipalName))
                cachedOwners.Add(userCache.Single(m => m.Key == owner.UserPrincipalName).Value);
            else
                taskOwners.Add(GetTeamMemberByPrincipalName(owner.UserPrincipalName));
        }

        var cachedMembers = new List<ITeamMember>();
        foreach (var memb in insegnamento.Members.Where(a => a != null && !string.IsNullOrWhiteSpace(a.UserPrincipalName)))
        {
            if (userCache.ContainsKey(memb.UserPrincipalName))
                cachedMembers.Add(userCache.Single(m => m.Key == memb.UserPrincipalName).Value);
            else
                taskMembers.Add(GetTeamMemberByPrincipalName(memb.UserPrincipalName));
        }

        var resultOwners = await Task.WhenAll(taskOwners);
        foreach (var item in resultOwners.Where(a => a != null && !string.IsNullOrWhiteSpace(a.UserPrincipalName)))
        {
            if (!userCache.ContainsKey(item.UserPrincipalName))
                userCache.Add(item.UserPrincipalName, item);
        };
        var resultMembers = await Task.WhenAll(taskMembers);
        foreach (var item in resultMembers.Where(a => a != null && !string.IsNullOrWhiteSpace(a.UserPrincipalName)))
        {
            if (!userCache.ContainsKey(item.UserPrincipalName))
                userCache.Add(item.UserPrincipalName, item);
        };

        insegnamento.Owners = resultOwners.Where(a => a != null).Concat(cachedOwners);
        insegnamento.Members = resultMembers.Where(a => a != null).Concat(cachedMembers);

        return insegnamento;
    }

    public async Task<IEducationalClassTeam> MapTeamOwnerMemberPrincipalName(IEducationalClassTeam insegnamento, Func<string, Task<ITeamMember>> retrieveUser)
    {
        async Task<IEnumerable<ITeamMember>> f(IEnumerable<ITeamMember> members)
        {
            var tt = new List<Task<ITeamMember>>();
            var cleanedMembers = members.Where(a => !string.IsNullOrWhiteSpace(a?.UserPrincipalName));
            foreach (var member in cleanedMembers)
            {
                tt.Add(retrieveUser(member.UserPrincipalName));
            }
            var result = await Task.WhenAll(tt);
            var cleanedResult = result.Where(s => s != null);
            return cleanedResult;
        }

        var tt = new Task<IEnumerable<ITeamMember>>[2] { f(insegnamento.Owners), f(insegnamento.Members) };
        await Task.WhenAll(tt);

        insegnamento.Owners = tt[0].Result;
        insegnamento.Members = tt[1].Result;
        return insegnamento;
    }

    public async Task<CallRecord> GetCallRecord(string callId, CancellationToken cancellationToken = default)
    {
        try
        {
            var res = await _retrier.Do(async () => await graphClient.Communications.CallRecords[$"{callId}"]
                .Request()
                .Expand("sessions($expand=segments)")
                .GetAsync(cancellationToken), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay), cancellationToken: cancellationToken);

            return res;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"GetCallRecord: CallRecord *probably* not found");
            return null;
        }
    }

    public async Task<IEnumerable<Session>> GetCallSessions(CallRecord callRecord, CancellationToken cancellationToken = default)
    {
        List<Session> sessionList = new List<Session>();
        try
        {
            ICallRecordSessionsCollectionPage sessionCollectionPage = callRecord.Sessions;
            sessionList.AddRange(sessionCollectionPage);
            ICallRecordSessionsCollectionRequest sessionCollectionRequest = sessionCollectionPage.NextPageRequest;
            while (sessionCollectionRequest != null)
            {
                ICallRecordSessionsCollectionPage sessionCollectionNextPage = await _retrier.Do(async () => await sessionCollectionRequest.GetAsync(cancellationToken)
                , TimeSpan.FromSeconds(_authenticationConfig.RetryDelay), cancellationToken: cancellationToken);
                sessionList.AddRange(sessionCollectionNextPage);
                sessionCollectionRequest = sessionCollectionNextPage.NextPageRequest;
            }

            return sessionList;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"GetCallSessions: Session *probably* not found");
            return sessionList;
        }
    }

    public async Task<IEnumerable<Segment>> GetCallSegment(Session callRecord, CancellationToken cancellationToken = default)
    {
        List<Segment> segmentList = new List<Segment>();
        try
        {
            ISessionSegmentsCollectionPage segmentCollectionPage = callRecord.Segments;
            segmentList.AddRange(segmentCollectionPage);
            ISessionSegmentsCollectionRequest segmentCollectionRequest = segmentCollectionPage.NextPageRequest;
            while (segmentCollectionRequest != null)
            {
                ISessionSegmentsCollectionPage segmentCollectionNextPage = await _retrier.Do(async () => await segmentCollectionRequest.GetAsync(cancellationToken)
                , TimeSpan.FromSeconds(_authenticationConfig.RetryDelay), cancellationToken: cancellationToken);
                segmentList.AddRange(segmentCollectionNextPage);
                segmentCollectionRequest = segmentCollectionNextPage.NextPageRequest;
            }

            return segmentList;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"GetCallSegment: Segment *probably* not found");
            return segmentList;
        }
    }

    public async Task<OnlineMeeting> GetOnlineMeeting(string userId, string joinWebUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            string filter = string.Format("JoinWebUrl%20eq%20'{0}'", joinWebUrl);

            var onlineMeetingsRequest = graphClient.Users[userId].OnlineMeetings
                .Request()
                .Filter(filter);
            var onlineMeetings = await onlineMeetingsRequest.GetAsync(cancellationToken);
            var json = System.Text.Json.JsonSerializer.Serialize(onlineMeetings);
            return onlineMeetings.CurrentPage.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"GetOnlineMeeting: ");
            return null;
        }
    }

    public async Task<OnlineMeeting> CreateOnlineMeeting(string userId, OnlineMeeting onlineMeeting, CancellationToken cancellationToken = default)
    {
        try
        {
            return await graphClient.Users[userId]
                .OnlineMeetings
                .Request()
                .AddAsync(onlineMeeting, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"CreateOnlineMeeting: ");
            throw;
        }
    }

    public async Task<Event> CreateEvent(string userId, Event onlineMeeting, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _retrier.Do(async () => await graphClient.Users[userId]
                .Calendar
                .Events
                .Request()
                .AddAsync(onlineMeeting, cancellationToken)
                 , TimeSpan.FromSeconds(_authenticationConfig.RetryDelay), cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"CreateEvent: ");
            throw;
        }
    }

    public async Task<Event> UpdateEvent(string ownerAccount, Event graphOnlineInputEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _retrier.Do(async () => await graphClient.Users[ownerAccount]
                 .Calendar
                 .Events[graphOnlineInputEvent.Id]
                 .Request()
                 .UpdateAsync(graphOnlineInputEvent, cancellationToken)
                 , TimeSpan.FromSeconds(_authenticationConfig.RetryDelay), cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"UpdateEvent: ");
            throw;
        }
    }
    private const string PreferredTimeZone = "W. Europe Standard Time";
    public async Task<Event> GetEvent(string ownerAccount, string eventId, CancellationToken cancellationToken = default)
    {
        try
        {
            var onlineEventTask = graphClient.Users[ownerAccount]
                .Calendar
                .Events[eventId]
                .Request()
                .Header("Prefer", $"outlook.timezone=\"{PreferredTimeZone}\"")
                .Select(a => new
                {
                    a.OnlineMeetingProvider,
                    a.OnlineMeeting,
                    a.OnlineMeetingUrl,
                    a.IsOnlineMeeting,
                    a.Subject,
                    a.Start,
                    a.End,
                    a.Location,
                    a.Attendees,
                    a.Extensions,
                    a.Body,
                    a.ICalUId,
                    a.WebLink,
                    a.Organizer
                })
                ;

            var onlineEvent = await _retrier.Do(async () => await onlineEventTask.GetAsync(cancellationToken)
                , TimeSpan.FromSeconds(_authenticationConfig.RetryDelay), cancellationToken: cancellationToken);
            return onlineEvent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"UpdateEvent: ");
            throw;
        }
    }

    public Event DefaultOnlineMeetingEvent(string subject, string bodyHtml, DateTime start, DateTime end, string location = "")
    {

        var dateFomat = "yyyy-MM-ddTHH:mm:ss";
        return new Event()
        {
            Subject = subject,
            Body = new ItemBody
            {
                ContentType = BodyType.Html,
                Content = bodyHtml
            },
            Start = new DateTimeTimeZone
            {
                DateTime = start.ToString(dateFomat),//"2019-03-10T12:00:00",
                TimeZone = PreferredTimeZone
            },
            End = new DateTimeTimeZone
            {
                DateTime = end.ToString(dateFomat),//"2019-03-10T12:00:00",
                TimeZone = PreferredTimeZone
            },
            Location = new Location
            {
                DisplayName = location
            },
            Attendees = new List<Attendee>(),
            IsOnlineMeeting = true,
            OnlineMeetingProvider = OnlineMeetingProviderType.TeamsForBusiness
        };
    }

    public string BuildFilter(IEnumerable<(string property, string @operator, string value)> ps)
    {
        string result = string.Empty;
        bool firstValue = true;
        foreach (var item in ps)
        {
            if (!firstValue)
                result += " and ";

            var filter = $"{item.property} {item.@operator} '{item.value}'";
            result += filter;
            firstValue = false;
        }
        return result;
    }

    public async Task<IEnumerable<Event>> SearchEvent(string userId, string filter, string expand = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var eventsTask = graphClient.Users[userId]
                .Calendar
                .Events
                .Request()
                .Select(a => new
                {
                    a.OnlineMeetingProvider,
                    a.OnlineMeeting,
                    a.OnlineMeetingUrl,
                    a.IsOnlineMeeting,
                    a.Subject,
                    a.Start,
                    a.End,
                    a.Location,
                    a.Attendees,
                    a.ChangeKey,
                    a.Extensions,
                    a.Body,
                    a.ICalUId,
                    a.WebLink,
                    a.Organizer
                });

            if (!string.IsNullOrWhiteSpace(filter))
                eventsTask = eventsTask.Filter(filter);
            if (!string.IsNullOrWhiteSpace(expand))
                eventsTask = eventsTask.Expand(expand);

            var events = await _retrier.Do(async () => await eventsTask.GetAsync(cancellationToken)
                , TimeSpan.FromSeconds(_authenticationConfig.RetryDelay), cancellationToken: cancellationToken);
            List<Event> sessionList = new List<Event>();

            sessionList.AddRange(events);
            ICalendarEventsCollectionRequest sessionCollectionRequest = events.NextPageRequest;
            while (sessionCollectionRequest != null)
            {
                ICalendarEventsCollectionPage sessionCollectionNextPage = await _retrier.Do(async () =>
                    await sessionCollectionRequest.GetAsync(cancellationToken), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
                sessionList.AddRange(sessionCollectionNextPage);
                sessionCollectionRequest = sessionCollectionNextPage.NextPageRequest;
            }
            return sessionList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Events with Extension");
            throw;
        }
    }

    public async Task<IEnumerable<Event>> GetEventByUserAndExtension(string userId, string extensionName, DateTime startDate, int dayToSeek, CancellationToken cancellationToken = default)
    {
        string filter = string.Empty;
        //string filter = $"Extensions/any(f:f/id eq '{extensionName}')";
        string expand = $"Extensions($filter=id eq '{extensionName}')";
        string format = "yyyy-MM-ddThh:mm:ss";

        if (!string.IsNullOrWhiteSpace(filter))
            filter += " and ";

        var datefilter = $"start/dateTime ge '{startDate.Date.ToString(format).Replace("12", "00")}' and start/dateTime lt '{startDate.AddDays(dayToSeek).ToString(format).Replace("12", "00")}'";
        filter += datefilter;

        return await SearchEvent(userId, filter, expand, cancellationToken);
    }

    public async Task<IEnumerable<Event>> GetEventBySubject(string userId, string subject, CancellationToken cancellationToken = default)
    {
        string filter = $"subject eq '{subject}'";
        return await SearchEvent(userId, filter, cancellationToken: cancellationToken);
    }

    public async Task DeleteEvent(string ownerAccount, string eventId, CancellationToken cancellationToken = default)
    {
        try
        {
            await graphClient.Users[ownerAccount]
                .Calendar
                .Events[eventId]
                .Request()
                .DeleteAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"UpdateEvent: ");
            throw;
        }
    }

    public async Task<IEnumerable<OnlineMeeting>> GetOnlineMeetings(CancellationToken cancellationToken = default)
    {
        try
        {
            var onlineEvent = await graphClient.Communications.OnlineMeetings
                .Request()
                .GetAsync(cancellationToken);
            List<OnlineMeeting> result = new(onlineEvent);

            ICloudCommunicationsOnlineMeetingsCollectionRequest sessionCollectionRequest = onlineEvent.NextPageRequest;
            while (sessionCollectionRequest != null)
            {
                ICloudCommunicationsOnlineMeetingsCollectionPage sessionCollectionNextPage = await _retrier.Do(async () =>
                    await sessionCollectionRequest.GetAsync(cancellationToken), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
                result.AddRange(sessionCollectionNextPage);
                sessionCollectionRequest = sessionCollectionNextPage.NextPageRequest;
            }
            return onlineEvent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"UpdateEvent: ");
            throw;
        }
    }

    public async Task DeleteOnlineMeeting(string eventId, CancellationToken cancellationToken = default)
    {
        try
        {
            await graphClient.Communications.OnlineMeetings[eventId]
                .Request()
                .DeleteAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"UpdateEvent: ");
            throw;
        }
    }
}

