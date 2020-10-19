using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.Abstraction
{
    public interface IGraphApiManager
    {
        Task AddGroupMember(string groupid, string id, IEnumerable<DirectoryObject> ownersInGroup = null);
        Task AddGroupOwner(string groupid, string id, IEnumerable<DirectoryObject> ownersInGroup = null);
        Task<ListItem> AddListItem(string siteId, string listName, ListItem item);
        Task ArchiveTeam(string id);
        Task ConnectAsApplication();
        Task<Channel> CreateChannel(string groupId, Channel channel);
        Task<EducationClass> CreateEducationClass(EducationClass group);
        Task<Group> CreateGroup(Group group);
        Task<Team> CreateTeam(string groupId, Team team);
        Task<Team> CreateTeam(Team team);
        Task<TeamsTab> CreateWebTab(string groupId, string chanId, TeamsTab tab);
        Channel DefaultChannelFactory(string displayName, string description = null);
        EducationClass DefaultEducationalClassFactory(IEducationalClassTeam educationalClassTeam);
        Team DefaultTeamFactory(string groupId);
        TeamsTab DefaultWebTab(string tabName, string tabUrl);
        Task DeleteChannel(string groupId, string chanId);
        Task DeleteEducationalClassesByExternalIdPrefix(string externalIdPrefix);
        Task DeleteEducationClass(string id);
        Task<IEnumerable<DirectoryObject>> GetRecentlyDeletedGroupsByMailNickname(string groupEmail);
        Task<EducationClass> GetEducationClass(string id);
        Task<IEnumerable<EducationClass>> GetEducationClasses();
        Task<EducationClass> GetEducationClassWGroupWTeam(string id);
        Task<Group> GetGroup(string id);
        Task<IEnumerable<ListItem>> GetListItems(string siteId, string listName);
        Task<List> GetListSite(string siteId, string listName);
        Task<Site> GetSiteByUri(Uri uri);
        Task<Team> GetTeam(string id);
        Task<ITeamMember> GetTeamMemberByPrincipalName(string userPrincipalName);
        Task<Team> GetTeamWChannelsWTabs(string id);
        Task<User> GetUserIdByPrincipalName(string mail);
        Task<IEnumerable<User>> GetUsersIdByPrincipalName(IEnumerable<string> mails);
        Task<IGraphServiceGroupsCollectionPage> ListGroups();
        Task RemoveGroupMember(string groupId, string azureUserId);
        Task RemoveGroupOwner(string groupId, string azureUserId);
        Task<IEnumerable<Site>> SearchSiteByKeywork(string key);
        Task<Channel> UpdateChannel(string groupId, Channel source, Channel update);
        Task<EducationClass> UpdateEducationClass(string id, EducationClass educationClass);
        Task UpdateGroupMembers(string groupid, IEnumerable<ITeamMember> newMembers, IEnumerable<DirectoryObject> existingMembers);
        Task UpdateGroupOwners(string groupid, IEnumerable<ITeamMember> newOwners, IEnumerable<DirectoryObject> existingOwners);
        Task<FieldValueSet> UpdateListItem(string siteId, string listName, string itemId, FieldValueSet item);
        Task<Team> UpdateTeam(string groupId, Team team);
        Task<TeamsTab> UpdateWebTab(string groupId, string chanId, string tabId, TeamsTab tab);
        Task<ListItem> GetListItem(string siteId, string listName, string itemId);
        Task<IEnumerable<ListItem>> SearchListItemByIndexedField(string siteId, string listName, string field, string value);
        Task AddGroupOwners(string groupid, IEnumerable<string> ownerIds, IEnumerable<DirectoryObject> ownersInGroup = null);
        Task AddGroupMembers(string groupid, IEnumerable<string> memeberIds, IEnumerable<DirectoryObject> ownersInGroup = null);
        Task<IEnumerable<DirectoryObject>> GetTeamMembers(string id);
        Task SendMail(string senderEmailAddress, Message message);
    }

}
