using System;

namespace Proge.Teams.Edu.DAL.Entities
{
    public class CallUser
    {
        public Guid Id { get; set; }
        public UserRole UserRole { get; set; }
        public string DisplayName { get; set; }
        public Guid? UserTenantId { get; set; }

        public Guid CallRecordId { get; set; }
        public CallRecord CallRecord { get; set; }
    }

    public enum UserRole
    {
        Default = 0,
        Organizer = 1,
        Participant = 2
    }
}
