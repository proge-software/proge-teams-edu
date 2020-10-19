using System;

namespace Proge.Teams.Edu.DAL.Entities
{
    public class ChangeNotification : BaseEntity
    {
        public Guid? Id { get; set; }
        public string RawJson { get; set; }
        public Guid? SubscriptionId { get; set; }
        public Guid? TenantId { get; set; }
        public ChangeType? ChangeType { get; set; }
        public DateTimeOffset? SubscriptionExpirationDateTime { get; set; }
        public string Resource { get; set; }
        public string ODataType { get; set; }
        public string ODataId { get; set; }
    }

    public enum ChangeType
    {
        Created = 0,
        Updated = 1,
        Deleted = 2
    }
}