using System;

namespace Proge.Teams.Edu.DAL.Entities
{
    public class CallSegment
    {
        public Guid Id { get; set; }
        public DateTimeOffset? StartDateTime { get; set; }
        public DateTimeOffset? EndDateTime { get; set; }
        public string UserPlatform { get; set; }
        public string UserProductFamily { get; set; }

        public Guid CallSessionId { get; set; }
        public CallSession CallSession { get; set; }
        public Guid CallUserId { get; set; }
        public UserRole CallUserRole { get; set; }
    }
}
