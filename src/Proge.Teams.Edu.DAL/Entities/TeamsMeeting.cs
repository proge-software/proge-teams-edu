extern alias BetaLib;
using Beta = BetaLib.Microsoft.Graph;
using System;
using System.Text.Json;

namespace Proge.Teams.Edu.DAL.Entities
{
    public class TeamsMeeting
    {
        // Online Meeting's Subject 
        public string MeetingName { get; set; }
        public string JoinUrl { get; set; }
        public string MeetingId { get; set; }
        // Additional Id if needed (e.g. used by UniBo for Moodle's ID)
        public string MeetingIdPrimary { get; set; }
        // Additional Id if needed (e.g. used by UniBo for internal ID)
        public string MeetingIdSecondary { get; set; }
        // Online Meeting Subject
        public string MeetingExtendedName { get; set; }
        // Online Meeting Organizer's DisplayName
        public string OwnerExtended { get; set; }
        // Online Meeting Organizer's Upn
        public string OwnerUpn { get; set; }
        public string MeetingExtendedAttribute { get; set; }
        public string MeetingType { get; set; }
        public string MeetingHierarchy { get; set; }
        public string CustomAttribute { get; set; }
        // ChatInfo's ThreadId
        public string ChatThreadId { get; set; }
        // ChatInfo's MessageId
        public string ChatMessageId { get; set; }
        public string Attendees { get; set; }
        public DateTime? CreationDateTime { get; set; }

        public TeamsMeeting() { }
        public TeamsMeeting(Microsoft.Graph.OnlineMeeting meeting)
        {
            MeetingName = meeting.Subject;
            JoinUrl = meeting.JoinWebUrl;
            MeetingId = meeting.Id;
            MeetingExtendedName = meeting.Subject;
            OwnerExtended = meeting.Participants.Organizer.Identity.User.DisplayName;
            OwnerUpn = meeting.Participants.Organizer.Upn;
            ChatMessageId = meeting.ChatInfo?.MessageId;
            ChatThreadId = meeting.ChatInfo?.ThreadId;
            Attendees = JsonSerializer.Serialize(meeting.Participants.Attendees);
            CreationDateTime = meeting.CreationDateTime?.DateTime;
        }
    }
}
