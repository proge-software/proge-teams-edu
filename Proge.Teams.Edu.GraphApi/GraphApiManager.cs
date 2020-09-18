using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Proge.Teams.Edu.Abstraction;
using Proge.Teams.Edu.GraphApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
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
        private readonly ILogger<GraphApiManager> _logger;

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
        /// Method ConnectAsApplication: not used
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public async Task ConnectAsApplication()
        {
            try
            {
                // Login Azure AD
                authenticationResult = await app.AcquireTokenForClient(_authenticationConfig.ScopeList)
                    .ExecuteAsync();
            }
            catch (MsalServiceException ex)
            {
                // Case when ex.Message contains:
                // AADSTS70011 Invalid scope. The scope has to be of the form "https://resourceUrl/.default"
                // Mitigation: change the scope to be as expected
                throw ex;
            }
        }

        public async Task<IGraphServiceGroupsCollectionPage> ListGroups()
        {
            var groups = await graphClient.Groups
                .Request()
                .GetAsync();

            return groups;
        }

        public async Task<User> GetUserIdByPrincipalName(string mail)
        {
            string filter = $"userPrincipalName eq '{mail}'";

            var users = await graphClient.Users
                .Request()
                .Filter(filter)
                .Select("id,userPrincipalName,jobTitle,officeLocation,department,mail,assignedLicenses")
                .GetAsync();


            //Fare un check sulle assignedLiceses perchè solo gli A1+ hanno Teams

            return users.FirstOrDefault();
        }

        public async Task<ITeamMember> GetTeamMemberByPrincipalName(string userPrincipalName)
        {
            string filter = $"userPrincipalName eq '{userPrincipalName}'";

            try
            {
                var users = await graphClient.Users
                       .Request()
                       .Filter(filter)
                       .Select("id,userPrincipalName,jobTitle,officeLocation,department,mail,assignedLicenses")
                       .GetAsync();


                //Fare un check sulle assignedLiceses perchè solo gli A1+ hanno Teams
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
                    };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"User {userPrincipalName} not found in Azure AD");
                return null;
            }
        }

        public async Task AddGroupOwners(string groupid, IEnumerable<string> ownerIds, IEnumerable<DirectoryObject> ownersInGroup = null)
        {
            foreach (var item in ownerIds.Distinct())
            {
                await AddGroupOwner(groupid, item, ownersInGroup);
            }
        }

        public async Task AddGroupOwner(string groupid, string id, IEnumerable<DirectoryObject> ownersInGroup = null)
        {
            if (ownersInGroup != null && ownersInGroup.Any(a => a.Id == id))
                return;

            var directoryObject = new DirectoryObject { Id = id };
            await Retry.Do<Task>(async () => await graphClient.Groups[$"{groupid}"].Owners.References
               .Request()
               .AddAsync(directoryObject), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
            ;
        }

        public async Task AddGroupMembers(string groupid, IEnumerable<string> memeberIds, IEnumerable<DirectoryObject> ownersInGroup = null)
        {
            foreach (var item in memeberIds.Distinct())
            {
                await AddGroupMember(groupid, item, ownersInGroup);
            }
        }

        public async Task AddGroupMember(string groupid, string id, IEnumerable<DirectoryObject> ownersInGroup = null)
        {
            if (ownersInGroup != null && ownersInGroup.Any(a => a.Id == id))
                return;

            var directoryObject = new DirectoryObject { Id = id };
            await Retry.Do<Task>(async () => await graphClient.Groups[$"{groupid}"].Members.References
               .Request()
               .AddAsync(directoryObject), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
            ;
        }

        public async Task RemoveGroupMember(string groupId, string azureUserId)
        {
            await graphClient.Groups[$"{groupId}"].Members[$"{azureUserId}"].Reference
                .Request()
                .DeleteAsync();
        }

        public async Task RemoveGroupOwner(string groupId, string azureUserId)
        {
            await graphClient.Groups[$"{groupId}"].Owners[$"{azureUserId}"].Reference
                .Request()
                .DeleteAsync();
        }

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

        public async Task<Team> CreateTeam(string groupId, Team team)
        {
            var resTeam = await graphClient.Groups[$"{groupId}"].Team
                .Request()
                .PutAsync(team);
            return resTeam;
        }

        public async Task<Team> UpdateTeam(string groupId, Team team)
        {
            var resTeam = await graphClient.Groups[$"{groupId}"].Team
                .Request()
                .UpdateAsync(team);
            return resTeam;
        }

        public async Task<Channel> CreateChannel(string groupId, Channel channel)
        {
            var resChannel = await graphClient.Groups[$"{groupId}"].Team.Channels
                .Request()
                .AddAsync(channel);
            return resChannel;
        }

        public async Task<Channel> UpdateChannel(string groupId, Channel source, Channel update)
        {
            if (source.DisplayName == update.DisplayName)
                return source;

            var resChannel = await graphClient.Groups[$"{groupId}"].Team.Channels[$"{source.Id}"]
                .Request()
                .UpdateAsync(update);
            return resChannel;
        }

        public async Task DeleteChannel(string groupId, string chanId)
        {
            await graphClient.Groups[$"{groupId}"].Team.Channels[$"{chanId}"]
                .Request()
                .DeleteAsync();
        }

        public async Task<Group> CreateGroup(Group group)
        {
            var resGroup = await graphClient.Groups
                .Request()
                .AddAsync(group);
            return resGroup;
        }

        public async Task<EducationClass> CreateEducationClass(EducationClass group)
        {
            return await Retry.Do(async () => await graphClient.Education.Classes
                .Request()
                .AddAsync(group), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
        }

        public async Task<Team> CreateTeam(Team team)
        {
            return await Retry.Do(async () => await graphClient.Teams
                .Request()
                .AddAsync(team), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
        }

        public async Task<TeamsTab> CreateWebTab(string groupId, string chanId, TeamsTab tab)
        {
            return await Retry.Do(async () => await graphClient.Teams[$"{groupId}"]
                .Channels[$"{chanId}"]
                .Tabs
               .Request()
               .AddAsync(tab), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
        }

        public async Task<TeamsTab> UpdateWebTab(string groupId, string chanId, string tabId, TeamsTab tab)
        {
            tab.ODataBind = null;
            return await Retry.Do(async () => await graphClient.Teams[$"{groupId}"]
                .Channels[$"{chanId}"]
                .Tabs[$"{tabId}"]
                .Request()
                .UpdateAsync(tab), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
        }

        public async Task<Group> GetGroup(string id)
        {
            string filter = $"id eq '{id}'";
            var resGroup = await graphClient.Groups
                .Request()
                .Filter(filter)
                .GetAsync();
            return resGroup.FirstOrDefault();
        }

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

        public async Task<Team> GetTeam(string id)
        {
            var resGroup = await Retry.Do(async () => await graphClient.Teams[$"{id}"]
                .Request()
                .GetAsync(), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
            return resGroup;
        }

        public async Task<IEnumerable<DirectoryObject>> GetTeamMember(string id)
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
                        .GetAsync(), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Team non trovato nel gruppo {id}");
            }            

            var members = await GetTeamMember(id);
            result.Group.Members.Clear();
            members.ToList().ForEach(a => result.Group.Members.Add(a));

            var owners = await Retry.Do(async () => await graphClient.Groups[$"{id}"].Owners
                .Request()
                .GetAsync(), TimeSpan.FromSeconds(_authenticationConfig.RetryDelay));

            result.Group.Owners = owners;

            return result;
        }

        public async Task<Team> GetTeamWChannelsWTabs(string id)
        {
            var result = await GetTeam(id);
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

        public async Task<EducationClass> GetEducationClass(string id)
        {
            return await graphClient.Education.Classes[$"{id}"]
             .Request()
             .GetAsync();
        }

        public async Task<EducationClass> UpdateEducationClass(string id, EducationClass educationClass)
        {
            return await graphClient.Education.Classes[$"{id}"]
               .Request()
               .UpdateAsync(educationClass);
        }

        public async Task DeleteEducationClass(string id)
        {
            await graphClient.Education.Classes[$"{id}"]
             .Request()
             .DeleteAsync();
        }

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

        public Channel DefaultChannelFactory(string displayName, string description = null)
        {
            return new Channel
            {
                DisplayName = displayName,
                Description = string.IsNullOrWhiteSpace(description) ? displayName : description
            };
        }

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
        /// NOT WORKING
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

            //Fare un check sulle assignedLiceses perchè solo gli A1+ hanno Teams

            return users;
        }

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

        public async Task<IEnumerable<ListItem>> GetListItems(string siteId, string listName)
        {
            var list = await graphClient.Sites[siteId].Lists[listName].Items
                .Request()
                .GetAsync();
            return list;
        }

        public async Task<ListItem> GetListItem(string siteId, string listName, string itemId)
        {
            var list = await graphClient.Sites[siteId].Lists[listName].Items[itemId]
                .Request()
                .GetAsync();
            return list;
        }

        public async Task<IEnumerable<ListItem>> SearchListItemByIndexedField(string siteId, string listName, string columnId, string value)
        {
            string filter = $"fields/{columnId} eq '{value}'";
            var list = await graphClient.Sites[siteId].Lists[listName].Items
                .Request()
                 .Filter(filter)
                .GetAsync();
            return list;
        }

        public async Task<ListItem> AddListItem(string siteId, string listName, ListItem item)
        {
            var list = await graphClient.Sites[siteId].Lists[listName].Items
                .Request()
                .AddAsync(item);
            return list;
        }

        public async Task<FieldValueSet> UpdateListItem(string siteId, string listName, string itemId, FieldValueSet item)
        {
            var list = await graphClient.Sites[siteId].Lists[listName].Items[itemId].Fields
                .Request()
                .UpdateAsync(item);
            return list;
        }

        public async Task<List> GetListSite(string siteId, string listName)
        {
            var list = await graphClient.Sites[siteId].Lists[listName]
                .Request()
                .GetAsync();
            return list;
        }

        public async Task<Site> GetSiteByUri(Uri uri)
        {
            var site = await graphClient.Sites.GetByPath(uri.DnsSafeHost, uri.AbsolutePath)
                .Request()
                .GetAsync();
            return site;
        }

        public async Task<IEnumerable<Site>> SearchSiteByKeywork(string key)
        {
            var queryOptions = new List<QueryOption>() { new QueryOption("search", key) };
            var site = await graphClient.Sites
                .Request(queryOptions)
                .GetAsync();
            return site;
        }
    }
}
