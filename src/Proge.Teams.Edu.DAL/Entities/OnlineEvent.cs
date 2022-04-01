using Proge.Teams.Edu.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.DAL.Entities
{
    public class OnlineEvent : BaseEntity
    {
        public string Id { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Location { get; set; }
        public string ICalUId { get; set; }
        public string JoinUrl { get; set; }
        public string WebLink { get; set; }
        public string ChangeKey { get; set; }

        public ICollection<OnlineEventAttendee> OnlineEventAttendee { get; set; }
    }
}
