using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.DAL.Entities
{
    public class OnlineEventAttendee
    {
        public string OnlineEventId { get; set; }
        public string AttendeeMail { get; set; }
        public OnlineEvent OnlineEvent { get; set; }
        public Attendee Attendee { get; set; }
        public AttendeeType AttendeeType { get; set; }
    }

    public enum AttendeeType
    {
        Required,
        Optional,
        Resource
    }
}
