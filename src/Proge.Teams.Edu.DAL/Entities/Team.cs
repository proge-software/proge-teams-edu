using System;
using System.Collections.Generic;

namespace Proge.Teams.Edu.DAL.Entities
{
    public class Team : BaseEntity
    {
        public Guid TeamsId { get; set; }
        public Guid? TenantId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ExternalId { get; set; }
        public string InternalId { get; set; }
        public string JoinCode { get; set; }
        public string JoinUrl { get; set; }
        public string TeamType { get; set; }
        public bool IsMembershipLimitedToOwners { get; set; }
        public DateTime? CreatedOn { get; set; }
        public ICollection<TeamMember> TeamsUsers { get; set; }
        public bool IsArchived { get; set; }
        public string DepartmentId { get; set; }
        public string AdId { get; set; }
        public string AdCod { get; set; }
        public string AdDesc { get; set; }
        public string AnnoOrdinamento { get; set; }
        public string CdsCod { get; set; }
        public string AnnoOfferta { get; set; }
        public string AdditionalDataString { get; set; }
    }
}
