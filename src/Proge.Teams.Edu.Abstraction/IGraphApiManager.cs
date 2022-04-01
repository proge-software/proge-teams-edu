using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Graph.CallRecords;

namespace Proge.Teams.Edu.Abstraction
{
    public interface IGraphApiManager
    {
        Task AddGroupMember(string groupid, string id, IEnumerable<DirectoryObject> ownersInGroup = null);
        Task AddGroupOwner(string groupid, string id, IEnumerable<DirectoryObject> ownersInGroup = null);
        Task<ListItem> AddListItem(string siteId, string listName, ListItem item);
        Task ArchiveTeam(string id, bool? shouldSetSpoSiteReadOnlyForMembers = null);
        Task UnArchiveTeam(string id);
        Task<Channel> CreateChannel(string groupId, Channel channel);
        Task<EducationClass> CreateEducationClass(EducationClass group);
        Task<Group> CreateGroup(Group group);
        Task<Team> CreateTeam(string groupId, Team team);
        Task<Team> CreateTeam(Team team);
        Task<TeamsTab> CreateWebTab(string teamId, string chanId, TeamsTab tab, CancellationToken cancellationToken = default);
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
        Task<EducationClass> GetEducationClassByMailNickname(string mailNickname);
        Task<Group> GetGroup(string id);
        Task<IEnumerable<ListItem>> GetListItems(string siteId, string listName);
        Task<IEnumerable<ListItem>> GetListItemsWithFieldsValues(string siteId, string listId, IEnumerable<QueryOption> queryOptions = null, string filter = null);
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
        Task<TeamsTab> UpdateWebTab(string groupId, string chanId, string tabId, TeamsTab tab, CancellationToken cancellationToken = default);
        Task<ListItem> GetListItem(string siteId, string listName, string itemId);
        Task<IEnumerable<ListItem>> SearchListItemByIndexedField(string siteId, string listName, string field, string value);
        Task<IEnumerable<ListItem>> SearchListItemDetailsByIndexedField(string siteId, string listName, string columnId, string value);
        Task AddGroupOwners(string groupid, IEnumerable<string> ownerIds, IEnumerable<DirectoryObject> ownersInGroup = null);
        Task AddGroupMembers(string groupid, IEnumerable<string> memeberIds, IEnumerable<DirectoryObject> ownersInGroup = null);
        Task<IEnumerable<DirectoryObject>> GetTeamMembers(string id);
        Task SendMail(string senderEmailAddress, Message message, bool? SaveToSentItems = null);
        Task<IEducationalClassTeam> MapTeamOwnerMemberPrincipalName(IEducationalClassTeam insegnamento, IDictionary<string, ITeamMember> userCache);
        Task<IEducationalClassTeam> MapTeamOwnerMemberPrincipalName(IEducationalClassTeam insegnamento, Func<string, Task<ITeamMember>> accessCache);
        Task<Subscription> AddSubscription(string changeType, string resource, DateTimeOffset? expirationOffset, string clientStateSecret,
            string notificationUrl, CancellationToken cancellationToken = default);        
        Task<bool> RenewSubscriptions();
        Task<Subscription> RenewSubscription(string id, int expirationOffsetDays = 2, CancellationToken cancellationToken = default);
        Task<Subscription> RenewSubscription(string id, DateTimeOffset expirationDateTime, CancellationToken cancellationToken = default);
        Task DeleteSubscription(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Subscription>> GetSubscriptions(CancellationToken cancellationToken = default);
        Task<CallRecord> GetCallRecord(string callId, CancellationToken cancellationToken = default);
        Task<OnlineMeeting> GetOnlineMeeting(string userId, string joinWebUrl, CancellationToken cancellationToken = default);
        Task<IEnumerable<Session>> GetCallSessions(CallRecord callRecord, CancellationToken cancellationToken = default);
        Task<IEnumerable<Segment>> GetCallSegment(Session callRecord, CancellationToken cancellationToken = default);
        Task<OnlineMeeting> CreateOnlineMeeting(string userId, OnlineMeeting onlineMeeting, CancellationToken cancellationToken = default);
        Task<Event> CreateEvent(string userId, Event onlineMeeting, CancellationToken cancellationToken = default);
        Task<Event> GetEvent(string ownerAccount, string eventId, CancellationToken cancellationToken = default);
        Task DeleteEvent(string ownerAccount, string eventId, CancellationToken cancellationToken = default);
        Event DefaultOnlineMeetingEvent(string subject, string bodyHtml, DateTime start, DateTime end, string location = "");
        Task<IEnumerable<Event>> GetEventByUserAndExtension(string userid, string extensionName, DateTime startDate, int dayToSeek, CancellationToken cancellationToken = default);
        Task<IEnumerable<Event>> GetEventBySubject(string userId, string subject, CancellationToken cancellationToken = default);
        Task<IEnumerable<Event>> SearchEvent(string userId, string filter, string expand = null, CancellationToken cancellationToken = default);
        string BuildFilter(IEnumerable<(string property, string @operator, string value)> ps);
        Task<Event> UpdateEvent(string ownerAccount, Event graphOnlineInputEvent, CancellationToken cancellationToken = default);
        Task<IEnumerable<OnlineMeeting>> GetOnlineMeetings(CancellationToken cancellationToken = default);
        Task DeleteOnlineMeeting(string eventId, CancellationToken cancellationToken = default);
    }
}
