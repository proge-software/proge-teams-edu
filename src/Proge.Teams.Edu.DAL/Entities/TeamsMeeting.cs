using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.DAL.Entities
{
    public class TeamsMeeting
    {
        public string MeetingName { get; set; }
        public string JoinUrl { get; set; }
        public string MeetingId { get; set; }
        public string MeetingIdPrimary { get; set; }
        public string MeetingIdSecondary { get; set; }
        public string MeetingExtendedName { get; set; }
        public string OwnerExtended { get; set; }
        public string OwnerUpn { get; set; }
        public string MeetingExtendedAttribute { get; set; }
        public string MeetingType { get; set; }
        public string MeetingHierarchy { get; set; }
        public string CustomAttribute { get; set; }
    }
}
