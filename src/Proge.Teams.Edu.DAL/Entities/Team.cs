using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
