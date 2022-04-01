using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.DAL.Entities
{
    public class Attendee : BaseEntity
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public ICollection<OnlineEventAttendee> OnlineEventAttendees { get; set; }
    }
}
