extern alias BetaLib;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Proge.Teams.Edu.Abstraction;
using Proge.Teams.Edu.DAL.Entities;
using Proge.Teams.Edu.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Beta = BetaLib.Microsoft.Graph;
using DalCallRecord = Proge.Teams.Edu.DAL.Entities.CallRecord;


namespace Proge.Teams.Edu.TeamsDashaborad
{
    public interface ITeamsDataCollectorManager
    {
        Task SubscribeCallRecords();
        Task ProcessNotification(Stream body);
        Task<(bool IsSuccess, string RetMessage)> WriteTeamsMeetingTable(Stream body, string senderCheckKey);
        Task<(bool IsSuccess, string RetMessage)> ProcessChangeNotification();
    }

    public class TeamsDataCollectorManager : ITeamsDataCollectorManager
    {
        protected readonly IBetaGraphApiManager betaGraphApiManager;
        protected readonly ILogger<TeamsDataCollectorManager> _logger;
        protected readonly UniSettings uniSettings;
        protected readonly CallFilters callFilters;
        protected readonly ICallRecordRepository _callRecordRepo;
        public TeamsDataCollectorManager(IOptions<UniSettings> uniCfg,
            IOptions<CallFilters> callFilter,
            IBetaGraphApiManager betaGraphApi,
            ILogger<TeamsDataCollectorManager> logger,
            ICallRecordRepository ichangenotrep)
        {
            betaGraphApiManager = betaGraphApi;
            uniSettings = uniCfg.Value;
            _logger = logger;
            callFilters = callFilter.Value;
            _callRecordRepo = ichangenotrep;
        }


        public virtual async Task<(bool IsSuccess, string RetMessage)> WriteTeamsMeetingTable(Stream body, string senderCheckKey)
        {
            try
            {
                bool bErr = false;
                string errMsgs = "WriteTeamsMeetingTable:";
                string retMessage = string.Empty;

                if (senderCheckKey == uniSettings.SenderKey)
                {
                    using (StreamReader reader = new StreamReader(body, Encoding.UTF8, true, 1024, true))
                    {
                        var bodyStr = reader.ReadToEnd();
                        _logger.LogInformation(bodyStr);
                        var receivedTeamsMeetings = JsonConvert.DeserializeObject<IEnumerable<TeamsMeeting>>(bodyStr);

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
                                if (await _callRecordRepo.GetExistsAsync<TeamsMeeting>(m => m.JoinUrl == receivedTeamMeeting.JoinUrl))
                                {
                                    var teamMeetingOnDb = await _callRecordRepo.GetByIdAsync<TeamsMeeting, string>(receivedTeamMeeting.JoinUrl);
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

        public virtual async Task ProcessNotification(Stream body)
        {
            try
            {
                //Beta.CallRecords.CallRecord callRecord;
                using (StreamReader reader = new StreamReader(body, Encoding.UTF8, true, 1024, true))
                {
                    var bodyStr = reader.ReadToEnd();
                    var collection = JsonConvert.DeserializeObject<Beta.ChangeNotificationCollection>(bodyStr);

                    _logger.LogInformation(bodyStr);

                    // Parse the received notifications.
                    var plainNotifications = new Dictionary<string, Beta.ChangeNotification>();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    foreach (var notification in collection.Value.Where(x => x.EncryptedContent == null))
                    {
                        // Verify the current client state matches the one that was sent.
                        if (notification.ClientState.Equals(uniSettings.ClientStateSecret))
                        {
                            // Just keep the latest notification for each resource. No point pulling data more than once.
                            plainNotifications[notification.Resource] = notification;

                            var changeNot = new DAL.Entities.ChangeNotification()
                            {
                                Id = Guid.NewGuid(),
                                RawJson = bodyStr,
                                SubscriptionId = notification.SubscriptionId,
                                SubscriptionExpirationDateTime = notification.SubscriptionExpirationDateTime,
                                TenantId = notification.TenantId,
                                ChangeType = notification.ChangeType,
                                Resource = notification.Resource,
                                ODataType = notification.ResourceData != null ? notification.ResourceData.ODataType : notification.ODataType,
                                ODataId = notification.ResourceData != null
                                && notification.ResourceData.AdditionalData != null
                                && !string.IsNullOrWhiteSpace(notification.ResourceData.AdditionalData["id"].ToString()) ?
                                notification.ResourceData.AdditionalData["id"].ToString() : string.Empty
                            };

                            DalCallRecord mappedCallRecord = new DalCallRecord();
                            if (!string.IsNullOrWhiteSpace(changeNot.ODataId))
                            {
                                Beta.CallRecords.CallRecord receivedCallRecord = await betaGraphApiManager.GetCallRecord(changeNot.ODataId);

                                // Save CallRecord, if not to discard according to config filters
                                if (await CallToSave(receivedCallRecord))
                                {
                                    mappedCallRecord = await MapReceivedCallRecord(receivedCallRecord);

                                    await _callRecordRepo.CreateAsync(mappedCallRecord);
                                    await _callRecordRepo.InsertChangeNotification(changeNot);
                                    await _callRecordRepo.SaveAsync();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ProcessNotification: { ex.Message }");
            }
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

            var changeNotificationCollection = await _callRecordRepo.GetAllAsync<ChangeNotification>();

            var callIdCollection = changeNotificationCollection.Select(c => c.ODataId);
            totCalls = callIdCollection.Count();
            callIdCollection = callIdCollection.Distinct().Select(c => c);
            totDistinctCalls = callIdCollection.Count();

            _logger.LogInformation($"ProcessChangeNotification tot calls to process: {totCalls}.");

            List<string> discardedByFiltersCallsODataIds = new List<string>();
            List<DalCallRecord> mappedCallRecsToSave = new List<DalCallRecord>();
            foreach (var callId in callIdCollection)
            {
                try
                {
                    Beta.CallRecords.CallRecord receivedCallRecord = await betaGraphApiManager.GetCallRecord(callId);

                    if (receivedCallRecord != null)
                    {
                        DalCallRecord mappedCallRecord = new DalCallRecord();

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
                                    await _callRecordRepo.BulkWriteCallRecordAsync(mappedCallRecsToSave);
                                    savedCalls += mappedCallRecsToSave.Count();
                                    mappedCallRecsToSave.Clear();

                                    Console.WriteLine($"mappedCalls (written in ChangeNotification db table): { mappedCalls }");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error during BulkWriteCallRecordAsync: { ex.Message }");
                                }
                            }
                        }
                        else
                        {
                            discardedByFiltersCallsODataIds.Add(callId);
                            callsToDiscard++;
                            if (callsToDiscard % 100 == 0)
                            {
                                Console.WriteLine($"callsToDiscard: { callsToDiscard }");
                            }
                        }
                    }
                    else
                    {
                        nullFromGraphApiCalls++;
                        if (nullFromGraphApiCalls % 10 == 0)
                        {
                            Console.WriteLine($"nullFromGraphApiCalls: { nullFromGraphApiCalls }");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"ProcessChangeNotification: { ex.Message }");
                    bulkListsCreatingCallsInError++;
                    Console.WriteLine($"ProcessChangeNotification: { ex.Message }");
                }
            }

            // Delete discarded calls from ChangeNotification
            int deletedFromChangeNotification = 0;
            IEnumerable<ChangeNotification> chnNotRowsToDelete = changeNotificationCollection.Where(n => discardedByFiltersCallsODataIds.Contains(n.ODataId));
            if (chnNotRowsToDelete != null && chnNotRowsToDelete.Any())
            {
                List<ChangeNotification> chnNotRowsToDeleteList = chnNotRowsToDelete.ToList();

                // EFCore.BulkExtensions bulk delete methods: not working
                //await _callRecordRepo.BulkDeleteChangeNotificationAsync(chnNotRowsToDeleteList);
                //_callRecordRepo.BulkDeleteChangeNotification(chnNotRowsToDeleteList);
                _callRecordRepo.DeleteAll(chnNotRowsToDeleteList);
                await _callRecordRepo.SaveAsync();
                deletedFromChangeNotification = chnNotRowsToDelete.Count();
            }

            // Insert/Update saved callRecs
            await _callRecordRepo.BulkWriteCallRecordAsync(mappedCallRecsToSave);
            savedCalls += mappedCallRecsToSave.Count();


            _logger.LogInformation("ProcessChangeNotification finished.");
            return (true, $"Calls found: {totCalls}; Deleted from ChangeNotification table: {deletedFromChangeNotification}; Distinct call id: {totDistinctCalls}; Null Calls from GraphApiCallDetail request: {nullFromGraphApiCalls}; Calls in error (in Api call or Api call result mapping): {bulkListsCreatingCallsInError}; Mapping errors: {callsToSave - mappedCalls}; Calls to discard due to filters: {callsToDiscard}; Calls to save: {callsToSave}; Saved calls: {savedCalls}.");
        }

        public virtual async Task SubscribeCallRecords()
        {
            //var subscription = new 

            bool renewedSubscription = await betaGraphApiManager.GetSubscriptions();

            if (!renewedSubscription)
            {
                var x = await betaGraphApiManager.AddSubscription(uniSettings.ChangeType, uniSettings.Resource, new DateTimeOffset(DateTime.UtcNow.AddDays(2)),
                    uniSettings.ClientStateSecret, uniSettings.NotificationUrl);
            }
        }

        /// <summary>
        /// Determines if the call must be saved or, depending on the filters values specified in config file, if it must be discarded.
        /// </summary>
        /// <param name="receivedCallRecord">CallRecord to check.</param>
        /// <returns>True if the CallRecord must be saved, false if it must be discarded.</returns>
        protected virtual async Task<bool> CallToSave(Beta.CallRecords.CallRecord receivedCallRecord)
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
                    Beta.CallRecords.CallType filterCallType = Beta.CallRecords.CallType.Unknown;
                    if (Enum.TryParse<Beta.CallRecords.CallType>(callFilters.CallType.Trim(), out filterCallType))
                    {
                        if (receivedCallRecord.Type.HasValue)
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
                    TeamsMeeting teamsMeeting = await _callRecordRepo.GetTeamsMeetingByJoinUrl(receivedCallRecord.JoinWebUrl);
                    saveCall = teamsMeeting != null && !string.IsNullOrWhiteSpace(teamsMeeting.MeetingName);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"ProcessNotification - error applying filters: { ex.Message }");
                return false;
            }

            return saveCall;
        }

        protected virtual async Task<DalCallRecord> MapReceivedCallRecord(Beta.CallRecords.CallRecord receivedCallRecord)
        {
            Guid callId = Guid.Parse(receivedCallRecord.Id);

            // Team Description
            string mappedDescription = string.Empty;
            TeamsMeeting teamsMeeting = await _callRecordRepo.GetTeamsMeetingByJoinUrl(receivedCallRecord.JoinWebUrl);
            if (teamsMeeting != null)
            {
                mappedDescription = teamsMeeting.MeetingId;
            }


            // Modalities
            string mappedModalities = string.Empty;
            foreach (var modality in receivedCallRecord.Modalities)
            {
                mappedModalities += $"{modality.ToString()},";
            }
            mappedModalities = mappedModalities.Substring(0, mappedModalities.Length - 1);

            // Users
            List<CallUser> mappedUsers = new List<CallUser>();
            // Users - Organizer
            if (receivedCallRecord.Organizer.User != null && receivedCallRecord.Organizer.User.DisplayName != null)
            {
                CallUser mappedOrganizer = MapReceivedUser(callId, receivedCallRecord.Organizer.User, UserRole.Organizer);
                mappedUsers.Add(mappedOrganizer);
            }
            // Users - Participants            
            foreach (var receivedParticipant in receivedCallRecord.Participants)
            {
                //UserRole role = (receivedParticipant.User.Id == receivedCallRecord.Organizer.User.Id) ? UserRole.Organizer : UserRole.Participant;
                if (receivedParticipant.User != null && !string.IsNullOrEmpty(receivedParticipant.User.DisplayName))
                {
                    mappedUsers.Add(MapReceivedUser(callId, receivedParticipant.User, DAL.Entities.UserRole.Participant));
                }
            }

            // Sessions with segments
            List<CallSession> mappedSessions = new List<CallSession>();
            ICollection<Beta.CallRecords.Session> receivedSessions = await BuildReceivedSessionList(receivedCallRecord);
            foreach (var receivedSession in receivedSessions)
            {
                mappedSessions.Add(await MapReceivedSession(callId, receivedSession));
            }

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
                CallSessions = mappedSessions
            };

            return mappedCallRec;
        }

        protected virtual CallUser MapReceivedUser(Guid callId, Beta.Identity receivedUser, DAL.Entities.UserRole userType)
        {
            CallUser mappedCallUser = new CallUser();

            mappedCallUser.Id = Guid.Parse(receivedUser.Id);
            mappedCallUser.CallRecordId = callId;
            mappedCallUser.UserRole = userType;
            mappedCallUser.DisplayName = receivedUser.DisplayName;

            if (receivedUser.AdditionalData["tenantId"] != null)
            {
                mappedCallUser.UserTenantId = string.IsNullOrWhiteSpace(receivedUser.AdditionalData["tenantId"].ToString()) ? new Nullable<Guid>() : Guid.Parse(receivedUser.AdditionalData["tenantId"].ToString());
            }

            return mappedCallUser;
        }

        protected virtual async Task<CallSession> MapReceivedSession(Guid callId, Beta.CallRecords.Session receivedSession)
        {
            CallSession mappedSession = new CallSession()
            {
                Id = Guid.Parse(receivedSession.Id),
                CallRecordId = callId,
                StartDateTime = receivedSession.StartDateTime,
                EndDateTime = receivedSession.EndDateTime
            };

            mappedSession.UserPlatform = ((Beta.CallRecords.ClientUserAgent)((Beta.CallRecords.ParticipantEndpoint)receivedSession.Caller).UserAgent).Platform.Value.ToString();
            mappedSession.UserProductFamily = ((Beta.CallRecords.ClientUserAgent)((Beta.CallRecords.ParticipantEndpoint)receivedSession.Caller).UserAgent).ProductFamily.Value.ToString();
            if (((Beta.CallRecords.ParticipantEndpoint)receivedSession.Caller).Identity.User != null)
            {
                mappedSession.CallUserId = Guid.Parse(((Beta.CallRecords.ParticipantEndpoint)receivedSession.Caller).Identity.User.Id);
            }
            mappedSession.CallUserRole = UserRole.Participant;

            // Session Segments
            List<CallSegment> mappedSegments = new List<CallSegment>();
            ICollection<Beta.CallRecords.Segment> receivedSegments = await BuildReceivedSegmentList(receivedSession);
            foreach (var receivedSegment in receivedSegments)
            {
                mappedSegments.Add(MapReceivedSegment(callId, mappedSession.Id, receivedSegment));
            }
            mappedSession.CallSegments = mappedSegments;

            return mappedSession;
        }

        protected virtual CallSegment MapReceivedSegment(Guid callId, Guid sessionId, Beta.CallRecords.Segment receivedSegment)
        {
            CallSegment mappedSegment = new CallSegment()
            {
                Id = Guid.Parse(receivedSegment.Id),
                CallSessionId = sessionId,
                StartDateTime = receivedSegment.StartDateTime,
                EndDateTime = receivedSegment.EndDateTime
            };

            mappedSegment.UserPlatform = ((Beta.CallRecords.ClientUserAgent)((Beta.CallRecords.ParticipantEndpoint)receivedSegment.Caller).UserAgent).Platform.Value.ToString();
            mappedSegment.UserProductFamily = ((Beta.CallRecords.ClientUserAgent)((Beta.CallRecords.ParticipantEndpoint)receivedSegment.Caller).UserAgent).ProductFamily.Value.ToString();
            if (((Beta.CallRecords.ParticipantEndpoint)receivedSegment.Caller).Identity.User != null)
            {
                mappedSegment.CallUserId = Guid.Parse(((Beta.CallRecords.ParticipantEndpoint)receivedSegment.Caller).Identity.User.Id);
            }
            mappedSegment.CallUserRole = UserRole.Participant;

            return mappedSegment;
        }

        private async Task<ICollection<Beta.CallRecords.Session>> BuildReceivedSessionList(Beta.CallRecords.CallRecord receivedCallRecord)
        {
            List<Beta.CallRecords.Session> sessionList = new List<Beta.CallRecords.Session>();

            Beta.CallRecords.ICallRecordSessionsCollectionPage sessionCollectionPage = receivedCallRecord.Sessions;
            sessionList.AddRange(sessionCollectionPage);
            Beta.CallRecords.ICallRecordSessionsCollectionRequest sessionCollectionRequest = sessionCollectionPage.NextPageRequest;
            while (sessionCollectionRequest != null)
            {
                Beta.CallRecords.ICallRecordSessionsCollectionPage sessionCollectionNextPage = await sessionCollectionRequest.GetAsync();
                sessionList.AddRange(sessionCollectionNextPage);
                sessionCollectionRequest = sessionCollectionNextPage.NextPageRequest;
            }

            return sessionList;
        }

        protected virtual async Task<ICollection<Beta.CallRecords.Segment>> BuildReceivedSegmentList(Beta.CallRecords.Session receivedSession)
        {
            List<Beta.CallRecords.Segment> segmentList = new List<Beta.CallRecords.Segment>();

            Beta.CallRecords.ISessionSegmentsCollectionPage segmentCollectionPage = receivedSession.Segments;
            segmentList.AddRange(segmentCollectionPage);
            Beta.CallRecords.ISessionSegmentsCollectionRequest segmentCollectionRequest = segmentCollectionPage.NextPageRequest;
            while (segmentCollectionRequest != null)
            {
                Beta.CallRecords.ISessionSegmentsCollectionPage segmentCollectionNextPage = await segmentCollectionRequest.GetAsync();
                segmentList.AddRange(segmentCollectionNextPage);
                segmentCollectionRequest = segmentCollectionNextPage.NextPageRequest;
            }

            return segmentList;
        }

    }
}