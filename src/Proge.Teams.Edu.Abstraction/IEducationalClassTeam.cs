using System;
using System.Collections.Generic;

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
        string JoinUrl { get; set; }
        bool? IsMembershipLimitedToOwners { get; set; }
        string TeamType { get; set; }
        IEnumerable<ITeamMember> Members { get; set; }
        IEnumerable<ITeamMember> Owners { get; set; }
        string DepartmentId { get; set; }
        string AdId { get; set; }
        string AdCod { get; set; }
        string AdDesc { get; set; }
        string CdsCod { get; set; }
        string AnnoOfferta { get; set; }
        string AnnoOrd { get; set; }
        string AdditionalDataString { get; set; }
    }
}
