using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.Abstraction
{
    public interface IEducationalClassTeam
    {
        Guid? Id { get; set; }
        string Key { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        string InternalId { get; set; }
        Guid? TenantId { get; set; }
        string JoinCode { get; set; }
        public string JoinUrl { get; set; }
        public bool? IsMembershipLimitedToOwners { get; set; }
        public string TeamType { get; set; }
        IEnumerable<ITeamMember> Members { get; set; }
        IEnumerable<ITeamMember> Owners { get; set; }
    }

    
}
