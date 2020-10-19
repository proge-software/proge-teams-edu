using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using Proge.Teams.Edu.Abstraction;
using Proge.Teams.Edu.GraphApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace Proge.Teams.Edu.GraphApi
{

    public class GraphApiManager : IGraphApiManager
    {
        private readonly AuthenticationConfig _authenticationConfig;
        private IConfidentialClientApplication app { get; set; }
        private ClientCredentialProvider authProvider { get; set; }
        private GraphServiceClient graphClient { get; set; }
        private AuthenticationResult authenticationResult { get; set; }
        private string _httpToken { get; set; }        
        private readonly ILogger<GraphApiManager> _logger;
        private static readonly HttpClient httpClient = new HttpClient();

        private static JsonSerializerOptions DefaultSerializerOption = new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            PropertyNameCaseInsensitive = true
        };


        public GraphApiManager(IOptions<AuthenticationConfig> authCfg, ILogger<GraphApiManager> logger)
        {
            _logger = logger;
            _authenticationConfig = authCfg.Value;
            app = ConfidentialClientApplicationBuilder.Create(_authenticationConfig.ClientId)
                  .WithAuthority(AzureCloudInstance.AzurePublic, _authenticationConfig.TenantId)
                  .WithClientSecret(_authenticationConfig.ClientSecret)
                  .Build();
            authProvider = new ClientCredentialProvider(app);
            graphClient = new GraphServiceClient(authProvider);
        }

        /// <summary>
        /// Establish the connection.
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsApplication()
        {
            try
            {
                // Login Azure AD
                authenticationResult = await app.AcquireTokenForClient(_authenticationConfig.ScopeList)
                    .ExecuteAsync();

                // Setting client default request headers
                _httpToken = authenticationResult.AccessToken;
                if (httpClient.DefaultRequestHeaders.Contains("Authentication"))
                {
                    httpClient.DefaultRequestHeaders.Remove("Authentication");
                }
                httpClient.DefaultRequestHeaders.Add("Authentication", $"Bearer {this._httpToken}");
                if (httpClient.DefaultRequestHeaders.Contains("Authorization"))
                {
                    httpClient.DefaultRequestHeaders.Remove("Authorization");
                }
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {this._httpToken}");
            }
            catch (MsalServiceException ex)
            {
                // Case when ex.Message contains:
                // AADSTS70011 Invalid scope. The scope has to be of the form "https://resourceUrl/.default"
                // Mitigation: change the scope to be as expected
                throw ex;
            }
        }

        //private async Task<bool> EnsureUnpToken()
        private bool EnsureUnpToken()
        {
            if (string.IsNullOrWhiteSpace(_httpToken))
            {
                throw new System.Security.Authentication.AuthenticationException("'Username & pwd' token is empty");
            }
            //else
            //{
            //    if (DateTime.Now < dtFirstStart.AddSeconds(3600) && DateTime.Now > dtNextConnRefresh)
            //    {
            //        await this.ConnectWithUnp();
            //        dtNextConnRefresh = dtNextConnRefresh.AddSeconds(connRefrSeconds);
            //    }
            //}

            return true;
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
                       .Select("id,userPrincipalName,givenName,surname")//,jobTitle,officeLocation,department,mail,assignedLicenses")
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
                await Retry.Do<Task>(async () => await graphClient.Groups[$"{groupid}"].Owners.References
                       .Request()
                       .AddAsync(directoryObject), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay), 2);
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
                await Retry.Do<Task>(async () => await graphClient.Groups[$"{groupid}"].Members.References
                       .Request()
                       .AddAsync(directoryObject), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay), 2);
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
            return await Retry.Do(async () => await graphClient.Education.Classes
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
            return await Retry.Do(async () => await graphClient.Teams
                .Request()
                .AddAsync(team), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
        }

        /// <summary>
        /// Adds (pins) a tab to the specified channel within a team.
        /// </summary>
        /// <param name="groupId">Id of the team.</param>
        /// <param name="chanId">Id of the channel.</param>
        /// <param name="tab">The Microsoft.Graph.TeamsTab to add.</param>
        /// <returns>The new Microsoft.Graph.TeamsTab object added to the channel.</returns>
        public async Task<TeamsTab> CreateWebTab(string groupId, string chanId, TeamsTab tab)
        {
            return await Retry.Do(async () => await graphClient.Teams[$"{groupId}"]
                .Channels[$"{chanId}"]
                .Tabs
               .Request()
               .AddAsync(tab), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
        }

        /// <summary>
        /// Update the properties of the specified tab.
        /// </summary>
        /// <param name="groupId">Id of the team.</param>
        /// <param name="chanId">Id of the channel.</param>
        /// <param name="tabId">Id of the tab.</param>
        /// <param name="tab">A Microsoft.Graph.TeamsTab object with the new values for relevant fields that should be updated.</param>
        /// <returns></returns>
        public async Task<TeamsTab> UpdateWebTab(string groupId, string chanId, string tabId, TeamsTab tab)
        {
            tab.ODataBind = null;
            return await Retry.Do(async () => await graphClient.Teams[$"{groupId}"]
                .Channels[$"{chanId}"]
                .Tabs[$"{tabId}"]
                .Request()
                .UpdateAsync(tab), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
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
        public async Task ArchiveTeam(string id)
        {
            try
            {
                await graphClient.Teams[$"{id}"]
                     .Archive(null)
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
                var resGroup = await Retry.Do(async () => await graphClient.Teams[$"{id}"]
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

            var classes = await Retry.Do(async () => await graphClient.Groups[$"{id}"].Members
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
            var result = await GetEducationClass(id);
            var group = await Retry.Do(async () => await graphClient.Groups[$"{id}"]
                .Request()
                .Expand("members")//,teams,owners")
                .GetAsync(), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
            result.Group = group;

            try
            {
                result.Group.Team = await Retry.Do(async () => await graphClient.Teams[$"{id}"]
                        .Request()
                        .GetAsync(), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay), 1);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Team not found in group {id}");
            }

            var members = await GetTeamMembers(id);
            result.Group.Members.Clear();
            members.ToList().ForEach(a => result.Group.Members.Add(a));

            var owners = await Retry.Do(async () => await graphClient.Groups[$"{id}"].Owners
                .Request()
                .GetAsync(), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));

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

            var channels = await Retry.Do(async () => await graphClient.Teams[$"{id}"].Channels
                    .Request()
                    .GetAsync(), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));

            result.Channels = channels;

            foreach (var item in channels)
            {
                var tabs = await Retry.Do(async () => await graphClient.Teams[$"{id}"]
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

                EnsureUnpToken();
                HttpResponseMessage response = await httpClient.GetAsync($"https://graph.microsoft.com/v1.0/directory/deletedItems/microsoft.graph.group?$Filter={filter}");

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
            return new EducationClass
            {
                MailNickname = educationalClassTeam.Key,
                DisplayName = educationalClassTeam.Name,
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
                    { "template@odata.bind", "https://graph.microsoft.com/beta/teamsTemplates('educationClass')"},
                    {"group@odata.bind", $"https://graph.microsoft.com/v1.0/groups('{groupId}')"}
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

            string query = string.Empty;
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
        public async Task SendMail(string senderEmailAddress, Message message)
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
    }
}
