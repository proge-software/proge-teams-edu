using System.Collections.Generic;

namespace Proge.Teams.Edu.Abstraction
{
    public class TeamsConfig
    {
        public IEnumerable<TeamUser> MandatoryTeamOwners { get; set; }
        public IEnumerable<TeamUser> MandatoryTeamMembers { get; set; }
    }

    public class TeamUser
    {
        public string UserPrincipalName { get; set; }
        public string Mail { get; set; }
    }
}
