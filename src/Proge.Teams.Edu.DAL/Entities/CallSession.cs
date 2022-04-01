using System;
using System.Collections.Generic;

namespace Proge.Teams.Edu.DAL.Entities
{
    public class CallSession
    {
        public Guid Id { get; set; }
        public DateTimeOffset? StartDateTime { get; set; }
        public DateTimeOffset? EndDateTime { get; set; }
        public string UserPlatform { get; set; }
        public string UserProductFamily { get; set; }

        public Guid CallRecordId { get; set; }
        public CallRecord CallRecord { get; set;}
        public Guid CallUserId { get; set; }
        public UserRole CallUserRole { get; set; }

        public ICollection<CallSegment> CallSegments { get; set; }
    }
}
