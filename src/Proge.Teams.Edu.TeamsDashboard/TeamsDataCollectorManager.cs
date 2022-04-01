extern alias BetaLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Proge.Teams.Edu.Abstraction;
using Proge.Teams.Edu.DAL.Repositories;
using Beta = BetaLib.Microsoft.Graph;
using DalCallRecord = Proge.Teams.Edu.DAL.Entities.CallRecord;

namespace Proge.Teams.Edu.TeamsDashboard
{
    public interface ITeamsDataCollectorManager
    {
        Task SubscribeCallRecords();
        Task<bool> ProcessNotification(Stream body);
        Task<(bool IsSuccess, string RetMessage)> WriteTeamsMeetingTable(Stream body, string senderCheckKey);
        [Obsolete("Use ProcessNotification")]
        Task<(bool IsSuccess, string RetMessage)> ProcessChangeNotification();
    }

    public class TeamsDataCollectorManager : ITeamsDataCollectorManager
    {
        protected readonly IGraphApiManager _graphApiManager;
        protected readonly ILogger<TeamsDataCollectorManager> _logger;
        protected readonly UniSettings _uniSettings;
        protected readonly CallFilters callFilters;
        protected readonly ICallRecordRepository _callRecordRepo;
        protected readonly ITeamsMeetingRepository _teamsMeetingRepository;

        public TeamsDataCollectorManager(
            IOptions<UniSettings> uniCfg,
            IOptions<CallFilters> callFilter,
            IGraphApiManager graphApi,
            ILogger<TeamsDataCollectorManager> logger,
            ICallRecordRepository ichangenotrep,
            ITeamsMeetingRepository teamsMeetingRepository)
        {
            _graphApiManager = graphApi;
            _uniSettings = uniCfg.Value;
            _logger = logger;
            callFilters = callFilter.Value;
            _callRecordRepo = ichangenotrep;
            _teamsMeetingRepository = teamsMeetingRepository;
        }

        public virtual async Task<(bool IsSuccess, string RetMessage)> WriteTeamsMeetingTable(Stream body, string senderCheckKey)
        {
            try
            {
                bool bErr = false;
                string errMsgs = "WriteTeamsMeetingTable:";
                string retMessage = string.Empty;

                if (senderCheckKey == _uniSettings.SenderKey)
                {
                    using (StreamReader reader = new StreamReader(body, Encoding.UTF8, true, 1024, true))
                    {
                        var bodyStr = reader.ReadToEnd();
                        _logger.LogInformation(bodyStr);
                        var receivedTeamsMeetings = JsonConvert.DeserializeObject<IEnumerable<DAL.Entities.TeamsMeeting>>(bodyStr);

                        foreach (var receivedTeamMeeting in receivedTeamsMeetings)
                        {

                            string value = receivedTeamMeeting.MeetingId;
                            string a = "meeting_";
                            string b = "@thread";

                            int posA = value.IndexOf(a);
                            int posB = value.LastIndexOf(b);
                            if (posA != -1 && posB != -1)
                            {
                                int adjustedPosA = posA + a.Length;

                                receivedTeamMeeting.MeetingId = adjustedPosA >= posB ? receivedTeamMeeting.MeetingId : value.Substring(adjustedPosA, posB - adjustedPosA);
                            }

                            if (string.IsNullOrWhiteSpace(receivedTeamMeeting.JoinUrl))
                            {
                                bErr = true;
                                string errMsg = $"Missing joinurl in http request body (entity meetingname: '{receivedTeamMeeting.MeetingName}', entity meetingid: '{receivedTeamMeeting.MeetingId}');";
                                _logger.LogError(errMsg);
                                errMsgs += $" {errMsg}";
                            }
                            else if (string.IsNullOrWhiteSpace(receivedTeamMeeting.MeetingName))
                            {
                                bErr = true;
                                string errMsg = $"Missing meetingname in http request body (entity joinurl: '{receivedTeamMeeting.JoinUrl}', entity meetingid: '{receivedTeamMeeting.MeetingId}');";
                                _logger.LogError(errMsg);
                                errMsgs += $" {errMsg}";
                            }
                            else
                            {
                                if (await _callRecordRepo.GetExistsAsync<DAL.Entities.TeamsMeeting>(m => m.JoinUrl == receivedTeamMeeting.JoinUrl))
                                {
                                    var teamMeetingOnDb = await _callRecordRepo.GetByIdAsync<DAL.Entities.TeamsMeeting, string>(receivedTeamMeeting.JoinUrl);
                                    teamMeetingOnDb = receivedTeamMeeting;
                                    _callRecordRepo.Update(teamMeetingOnDb);
                                    retMessage = "update";
                                }
                                else
                                {
                                    await _callRecordRepo.CreateAsync(receivedTeamMeeting);
                                }
                            }
                        }
                    }
                }
                else
                {
                    bErr = true;
                    errMsgs += " Wrong query-string parameter value;";
                }

                if (bErr)
                {
                    return (false, $"{errMsgs.Substring(0, errMsgs.Length - 1)}.");
                }
                else
                {
                    await _callRecordRepo.SaveAsync();
                    return (true, retMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"WriteTeamsMeetingTable: { ex.Message }");
                return (false, ex.Message);
            }
        }

        private static async Task<string> ReadStream(Stream body)
        {
            using StreamReader reader = new StreamReader(body, Encoding.UTF8, true, 1024, true);
            return await reader.ReadToEndAsync();
        }

        public virtual async Task<bool> ProcessNotification(Stream body)
        {
            string bodyStr = await ReadStream(body);
            try
            {
                var collection = System.Text.Json.JsonSerializer.Deserialize<Microsoft.Graph.ChangeNotificationCollection>(bodyStr);
                if (!collection?.Value?.Any() ?? true)
                {
                    _logger.LogInformation("Could not deserialize body into ChangeNotificationCollection: {0}", bodyStr);
                    return false;
                }

                var count = collection.Value.Count();
                _logger.LogDebug("Deserialized {0} Change Notification{1}", count, count > 1 ? "s" : string.Empty);

                // filter notification
                var filteredNotifications = collection.Value.Where(x
                    => x.EncryptedContent == null
                        && x.ClientState == _uniSettings.ClientStateSecret
                        && x.ResourceData?.AdditionalData != null && x.ResourceData.AdditionalData.ContainsKey("id")
                        );

                var fc = filteredNotifications?.Count();
                _logger.LogInformation("Filtered Change Notification{0} {1}/{2}", fc > 1 ? "s" : string.Empty, fc, count);

                foreach (Microsoft.Graph.ChangeNotification notification in filteredNotifications)
                {
                    await ProcessSingleNotification(bodyStr, notification);
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error processing notification: {0}", bodyStr);
                throw;
            }
        }

        private async Task ProcessSingleNotification(string bodyStr, Microsoft.Graph.ChangeNotification notification)
        {
            DAL.Entities.ChangeNotification changeNotification = new DAL.Entities.ChangeNotification
            {
                Id = Guid.NewGuid(),
                RawJson = bodyStr,
                SubscriptionId = notification.SubscriptionId,
                SubscriptionExpirationDateTime = notification.SubscriptionExpirationDateTime,
                TenantId = notification.TenantId,
                ChangeType = notification.ChangeType,
                Resource = notification.Resource,
                ODataType = notification.ResourceData.ODataType ?? notification.ODataType,
                ODataId = notification.ResourceData.AdditionalData["id"].ToString()
            };

            if (string.IsNullOrWhiteSpace(changeNotification.ODataId))
            {
                _logger.LogInformation("Change Notification without ODataId");
                return;
            }

            // Save CallRecord, if not to discard according to config filters
            Microsoft.Graph.CallRecords.CallRecord receivedCallRecord = await _graphApiManager.GetCallRecord(changeNotification.ODataId);
            if (receivedCallRecord == null || !await CallToSave(receivedCallRecord))
            {
                _logger.LogInformation("Call must not be saved: {0}", changeNotification.ODataId);
                return;
            }

            await PersistOnDatabase(receivedCallRecord, changeNotification);
            await SaveOnlineMeetingIfNeeded(receivedCallRecord);
        }

        private async Task PersistOnDatabase(Microsoft.Graph.CallRecords.CallRecord receivedCallRecord, DAL.Entities.ChangeNotification changeNotification)
        {
            // save call record
            DalCallRecord mappedCallRecord = await MapReceivedCallRecord(receivedCallRecord);
            await _callRecordRepo.CreateOrUpdate(mappedCallRecord, changeNotification);
        }

        private async Task SaveOnlineMeetingIfNeeded(Microsoft.Graph.CallRecords.CallRecord callRecord, CancellationToken cancellationToken = default)
        {
            if (!_uniSettings.SaveOnlineMeetingDetails)
                return;

            if (callRecord?.Organizer?.User == null
                || string.IsNullOrWhiteSpace(callRecord.Organizer.User.Id)
                || string.IsNullOrWhiteSpace(callRecord.JoinWebUrl))
            {
                _logger.LogWarning("SaveOnlineMeetingIfNeeded skipped: callRecord.Organizer.User.Id or callRecord.JoinWebUrl null");
                return;
            }

            try
            {
                await CreateOrUpdateOnlineMeeting(callRecord.Organizer.User.Id, callRecord.JoinWebUrl, cancellationToken);
                _logger.LogInformation("Created/Updated Online Meeting {0}", callRecord.JoinWebUrl);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating Online Meeting");
            }
        }

        private async Task CreateOrUpdateOnlineMeeting(string organizerId, string joinWebUrl, CancellationToken cancellationToken)
        {
            Microsoft.Graph.OnlineMeeting onlineMeeting = await _graphApiManager.GetOnlineMeeting(organizerId, joinWebUrl);
            if (onlineMeeting == null)
            {
                _logger.LogWarning(
                    "Can not retrieve details for online meeting: probably the meeting is older than 30 days and Microsoft Graph deleted the data."
                    + " Also ensure you have enabled on Azure the policy for the app registration to act as organizer with id {0}."
                    + " JoinWebUrl {1}",
                    organizerId, joinWebUrl);
                return;
            }

            bool exists = await _teamsMeetingRepository.ExistByJoinUrl(joinWebUrl, cancellationToken);
            if (!exists)
            {
                await CreateOnlineMeeting(onlineMeeting, cancellationToken);
                return;
            }

            await UpdateOnlineMeeting(onlineMeeting, cancellationToken);
        }
        private async Task CreateOnlineMeeting(Microsoft.Graph.OnlineMeeting onlineMeeting, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Creating Teams Meeting with id {0}", onlineMeeting.Id);
            var meeting = new DAL.Entities.TeamsMeeting(onlineMeeting);
            await _teamsMeetingRepository.Create(meeting, cancellationToken);
        }

        private async Task UpdateOnlineMeeting(Microsoft.Graph.OnlineMeeting onlineMeeting, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Updating Teams Meeting with id {0}", onlineMeeting.Id);
            var meeting = new DAL.Entities.TeamsMeeting(onlineMeeting);
            await _teamsMeetingRepository.UpdateMeeting(meeting, cancellationToken);
        }

        public virtual async Task<(bool IsSuccess, string RetMessage)> ProcessChangeNotification()
        {
            int totCalls = 0;
            int totDistinctCalls = 0;
            int callsToSave = 0;
            int mappedCalls = 0;
            int nullFromGraphApiCalls = 0;
            int callsToDiscard = 0;
            int savedCalls = 0;
            int bulkListsCreatingCallsInError = 0;

            _logger.LogInformation("ProcessChangeNotification started.");

            var changeNotificationCollection = await _callRecordRepo.GetAllAsync<DAL.Entities.ChangeNotification>();

            var callIdCollection = changeNotificationCollection.Select(c => c.ODataId);
            totCalls = callIdCollection.Count();
            callIdCollection = callIdCollection.Distinct().Select(c => c);
            totDistinctCalls = callIdCollection.Count();

            _logger.LogInformation($"ProcessChangeNotification tot calls to process: {totCalls}.");

            List<string> discardedByFiltersCallsODataIds = new();
            List<DalCallRecord> mappedCallRecsToSave = new();
            foreach (var callId in callIdCollection)
            {
                try
                {
                    Microsoft.Graph.CallRecords.CallRecord receivedCallRecord = await _graphApiManager.GetCallRecord(callId);

                    if (receivedCallRecord != null)
                    {
                        DalCallRecord mappedCallRecord = new();

                        if (await CallToSave(receivedCallRecord))
                        {
                            callsToSave++;
                            mappedCallRecord = await MapReceivedCallRecord(receivedCallRecord);
                            mappedCallRecsToSave.Add(mappedCallRecord);
                            mappedCalls++;
                            if (mappedCalls % 50 == 0)
                            {
                                try
                                {
                                    await _callRecordRepo.BulkWriteCallRecordsAsync(mappedCallRecsToSave);
                                    savedCalls += mappedCallRecsToSave.Count;
                                    mappedCallRecsToSave.Clear();

                                    _logger.LogDebug("mappedCalls (written in ChangeNotification db table): {0}", mappedCalls);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Error during BulkWriteCallRecordAsync");
                                }
                            }
                        }
                        else
                        {
                            discardedByFiltersCallsODataIds.Add(callId);
                            callsToDiscard++;
                            if (callsToDiscard % 100 == 0)
                            {
                                _logger.LogDebug("callsToDiscard: {0}", callsToDiscard);
                            }
                        }
                    }
                    else
                    {
                        nullFromGraphApiCalls++;
                        if (nullFromGraphApiCalls % 10 == 0)
                        {
                            _logger.LogDebug("nullFromGraphApiCalls: {0}", nullFromGraphApiCalls);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ProcessChangeNotification");
                    bulkListsCreatingCallsInError++;
                }
            }

            // Delete discarded calls from ChangeNotification
            int deletedFromChangeNotification = 0;
            IEnumerable<DAL.Entities.ChangeNotification> chnNotRowsToDelete = changeNotificationCollection.Where(n => discardedByFiltersCallsODataIds.Contains(n.ODataId));
            if (chnNotRowsToDelete != null && chnNotRowsToDelete.Any())
            {
                List<DAL.Entities.ChangeNotification> chnNotRowsToDeleteList = chnNotRowsToDelete.ToList();

                // EFCore.BulkExtensions bulk delete methods: not working
                //await _callRecordRepo.BulkDeleteChangeNotificationAsync(chnNotRowsToDeleteList);
                //_callRecordRepo.BulkDeleteChangeNotification(chnNotRowsToDeleteList);
                _callRecordRepo.DeleteAll(chnNotRowsToDeleteList);
                await _callRecordRepo.SaveAsync();
                deletedFromChangeNotification = chnNotRowsToDelete.Count();
            }

            // Insert/Update saved callRecs
            await _callRecordRepo.BulkWriteCallRecordsAsync(mappedCallRecsToSave);
            savedCalls += mappedCallRecsToSave.Count;


            _logger.LogInformation("ProcessChangeNotification finished.");
            return (true, $"Calls found: {totCalls}; Deleted from ChangeNotification table: {deletedFromChangeNotification}; Distinct call id: {totDistinctCalls}; Null Calls from GraphApiCallDetail request: {nullFromGraphApiCalls}; Calls in error (in Api call or Api call result mapping): {bulkListsCreatingCallsInError}; Mapping errors: {callsToSave - mappedCalls}; Calls to discard due to filters: {callsToDiscard}; Calls to save: {callsToSave}; Saved calls: {savedCalls}.");
        }

        public virtual async Task SubscribeCallRecords()
        {
            var subscriptions = await _graphApiManager.GetSubscriptions();
            var currentSubscription = subscriptions.FirstOrDefault(a =>
                a.Resource == _uniSettings.Resource
                && a.NotificationUrl == _uniSettings.NotificationUrl);


            var diffNotification = currentSubscription?.ExpirationDateTime?.LocalDateTime.Subtract(DateTimeOffset.Now.LocalDateTime).Hours;
            if (currentSubscription == null)
            {
                var _ = await _graphApiManager.AddSubscription(
                    _uniSettings.ChangeType,
                    _uniSettings.Resource,
                    new DateTimeOffset(DateTime.UtcNow.AddDays(2)),
                    _uniSettings.ClientStateSecret,
                    _uniSettings.NotificationUrl
                    );
            }
            //Diff between subscription datetime and now should be moved to configuration
            else if (diffNotification == null || diffNotification.Value < 6)
            {
                _logger.LogInformation("Subscription {0} will expire on {1}: Renewing...", currentSubscription.Id, currentSubscription.ExpirationDateTime.Value.ToString());
                await _graphApiManager.RenewSubscription(currentSubscription.Id);
            }
        }

        /// <summary>
        /// Determines if the call must be saved or, depending on the filters values specified in config file, if it must be discarded.
        /// </summary>
        /// <param name="receivedCallRecord">CallRecord to check.</param>
        /// <returns>True if the CallRecord must be saved, false if it must be discarded.</returns>
        protected virtual async Task<bool> CallToSave(Microsoft.Graph.CallRecords.CallRecord receivedCallRecord)
        {
            bool saveCall = true;

            try
            {
                // OrganizerId filter: save only call with a specific organizerId
                //if (!string.IsNullOrWhiteSpace(callFilters.OrganizerId))
                //{
                //    List<string> splitOrganizerId = callFilters.OrganizerId.Split(';').Select(p => p.Trim()).ToList();
                //    saveCall = splitOrganizerId.Count > 0 && splitOrganizerId.Contains(receivedCallRecord.Organizer.User.Id);
                //}


                // CallType filter
                if (saveCall && !string.IsNullOrWhiteSpace(callFilters.CallType))
                {
                    Microsoft.Graph.CallRecords.CallType filterCallType = Microsoft.Graph.CallRecords.CallType.Unknown;
                    if (Enum.TryParse<Microsoft.Graph.CallRecords.CallType>(callFilters.CallType.Trim(), out filterCallType))
                    {
                        if (receivedCallRecord != null && receivedCallRecord.Type.HasValue)
                            saveCall = receivedCallRecord.Type.Value.ToString().ToUpper() == callFilters.CallType.Trim().ToUpper();
                        else
                            throw new Exception($"Missing CallType value in received CallRecord: impossible to filter on this field.");
                    }
                    else
                        throw new Exception($"Wrong CallType filter value specified in settings config file: {callFilters.CallType} (it must match one of the existing Microsoft.Graph.CallRecords.CallType enum values).");
                }

                // MinDuration filter
                if (saveCall && callFilters.MinDuration.HasValue && receivedCallRecord.EndDateTime.HasValue && receivedCallRecord.StartDateTime.HasValue)
                {
                    var duration = receivedCallRecord.EndDateTime - receivedCallRecord.StartDateTime;
                    if (duration.Value.TotalSeconds < callFilters.MinDuration.Value)
                        saveCall = false;
                }

                // AtLeastOneUserWithoutUpn filter: save call only if there is at least one User without a specifc string in the DisplayName
                //if (saveCall && !string.IsNullOrWhiteSpace(callFilters.AtLeastOneUserWithoutUpn))
                //    saveCall = receivedCallRecord.Participants.Any(p => p.User != null && p.User.DisplayName != null && !p.User.DisplayName.Contains(callFilters.AtLeastOneUserWithoutUpn));

                // NameNeededInTeamsMeetingTable filter: if true, check if the meeting exist in the TeamsMeeting table before to save
                if (saveCall && callFilters.NameNeededInTeamsMeetingTable)
                {
                    var meetingName = await _callRecordRepo.GetTeamsMeetingNameByJoinUrl(receivedCallRecord.JoinWebUrl);
                    saveCall = !string.IsNullOrWhiteSpace(meetingName);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProcessNotification - error applying filters");
                return false;
            }

            return saveCall;
        }

        protected virtual async Task<DalCallRecord> MapReceivedCallRecord(Microsoft.Graph.CallRecords.CallRecord receivedCallRecord)
        {
            Guid callId = Guid.Parse(receivedCallRecord.Id);

            // Team Description
            DAL.Entities.TeamsMeeting teamsMeeting = await _callRecordRepo.GetTeamsMeetingByJoinUrl(receivedCallRecord.JoinWebUrl);
            string mappedDescription = teamsMeeting?.MeetingId ?? string.Empty;

            // Modalities
            string mappedModalities = string.Empty;
            foreach (var modality in receivedCallRecord.Modalities)
            {
                mappedModalities += $"{modality},";
            }
            mappedModalities = mappedModalities.Substring(0, mappedModalities.Length - 1);

            // Users
            List<DAL.Entities.CallUser> mappedUsers = new List<DAL.Entities.CallUser>();

            // Users - Organizer
            if (receivedCallRecord?.Organizer?.User != null
                && Guid.TryParse(receivedCallRecord.Organizer.User.Id, out Guid GraphUserId)
                && !string.IsNullOrWhiteSpace(receivedCallRecord.Organizer.User.DisplayName))
            {
                DAL.Entities.CallUser mappedOrganizer = MapReceivedUser(callId, receivedCallRecord.Organizer.User, DAL.Entities.UserRole.Organizer);
                mappedUsers.Add(mappedOrganizer);
            }

            // Users - Participants            
            foreach (var receivedParticipant in receivedCallRecord.Participants)
            {
                //UserRole role = (receivedParticipant.User.Id == receivedCallRecord.Organizer.User.Id) ? UserRole.Organizer : UserRole.Participant;
                if (!string.IsNullOrEmpty(receivedParticipant?.User?.DisplayName)
                    && Guid.TryParse(receivedParticipant.User.Id, out GraphUserId))
                {
                    mappedUsers.Add(MapReceivedUser(callId, receivedParticipant.User, DAL.Entities.UserRole.Participant));
                }
            }

            // Sessions with segments
            IEnumerable<Microsoft.Graph.CallRecords.Session> receivedSessions = await _graphApiManager.GetCallSessions(receivedCallRecord);
            ICollection<DAL.Entities.CallSession> mappedSessions = receivedSessions
                .Select(async r => await MapReceivedSession(callId, r))
                .Select(t => t.Result)
                .Where(x => x != null)
                .ToArray();

            _logger.LogInformation("Call {0} has {1} mapped sessions", callId, mappedSessions.Count);

            DalCallRecord mappedCallRec = new DalCallRecord()
            {
                Id = callId,
                Type = receivedCallRecord.Type.ToString(),
                JoinWebUrl = receivedCallRecord.JoinWebUrl,
                CallDescription = mappedDescription,
                StartDateTime = receivedCallRecord.StartDateTime,
                EndDateTime = receivedCallRecord.EndDateTime,
                Modalities = mappedModalities,
                CallUsers = mappedUsers,
                CallSessions = mappedSessions,
            };

            return mappedCallRec;
        }

        protected virtual DAL.Entities.CallUser MapReceivedUser(Guid callId, Microsoft.Graph.Identity receivedUser, DAL.Entities.UserRole userType)
        {
            try
            {
                DAL.Entities.CallUser mappedCallUser = new DAL.Entities.CallUser
                {
                    Id = Guid.Parse(receivedUser.Id),
                    CallRecordId = callId,
                    UserRole = userType,
                    DisplayName = receivedUser.DisplayName,
                };

                string tenantIdStr = receivedUser.AdditionalData["tenantId"].ToString();
                if (Guid.TryParse(tenantIdStr, out Guid tenantId))
                    mappedCallUser.UserTenantId = tenantId;

                return mappedCallUser;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error mapping user {0}", JsonConvert.SerializeObject(receivedUser));
                return null;
            }
        }

        protected virtual async Task<DAL.Entities.CallSession> MapReceivedSession(Guid callId, Microsoft.Graph.CallRecords.Session receivedSession)
        {
            if (!Guid.TryParse(receivedSession.Id, out Guid rId))
            {
                _logger.LogInformation("Can not map Session: invalid session Id ({0}) for call {1}", receivedSession.Id, callId);
                return null;
            }

            var caller = receivedSession.Caller as Microsoft.Graph.CallRecords.ParticipantEndpoint;
            var user = caller?.Identity?.User;
            if (!Guid.TryParse(user?.Id, out Guid callUserId))
            {
                _logger.LogInformation("Skipping session {0} because callUserId is not a valid GUID: {1}", rId, user?.Id);
                return null;
            }

            var userAgent = caller.UserAgent as Microsoft.Graph.CallRecords.ClientUserAgent;
            DAL.Entities.CallSession mappedSession = new DAL.Entities.CallSession
            {
                Id = rId,
                CallRecordId = callId,
                StartDateTime = receivedSession.StartDateTime,
                EndDateTime = receivedSession.EndDateTime,
                UserPlatform = userAgent?.Platform.Value.ToString() ?? "N/A",
                UserProductFamily = userAgent?.ProductFamily.Value.ToString() ?? "N/A",
                CallUserId = callUserId,
                CallUserRole = DAL.Entities.UserRole.Participant,
            };

            // Session Segments
            IEnumerable<Microsoft.Graph.CallRecords.Segment> receivedSegments = await _graphApiManager.GetCallSegment(receivedSession);
            mappedSession.CallSegments = receivedSegments
                .Select(r => MapReceivedSegment(callId, mappedSession.Id, r))
                .Where(x => x != null)
                .ToArray();

            return mappedSession;
        }

        protected virtual DAL.Entities.CallSegment MapReceivedSegment(Guid callId, Guid sessionId, Microsoft.Graph.CallRecords.Segment receivedSegment)
        {
            if (!Guid.TryParse(receivedSegment.Id, out Guid id))
            {
                _logger.LogWarning("Can not map segment because of invalid/null GUID id ({0}) for call {1}", receivedSegment.Id, callId);
                return null;
            }

            var caller = receivedSegment.Caller as Microsoft.Graph.CallRecords.ParticipantEndpoint;
            var user = caller?.Identity?.User;
            if (!Guid.TryParse(user?.Id, out Guid userId))
            {
                _logger.LogWarning("Skipping segment {0} because callUserId is not a valid GUID: {1}", id, user?.Id);
                return null;
            }

            var userAgent = caller.UserAgent as Microsoft.Graph.CallRecords.ClientUserAgent;
            DAL.Entities.CallSegment mappedSegment = new DAL.Entities.CallSegment
            {
                Id = id,
                CallSessionId = sessionId,
                StartDateTime = receivedSegment.StartDateTime,
                EndDateTime = receivedSegment.EndDateTime,
                UserPlatform = userAgent?.Platform.Value.ToString() ?? "N/A",
                UserProductFamily = userAgent?.ProductFamily.Value.ToString() ?? "N/A",
                CallUserRole = DAL.Entities.UserRole.Participant,
                CallUserId = userId,
            };

            return mappedSegment;
        }
    }
}