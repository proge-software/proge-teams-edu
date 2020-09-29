using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Proge.Teams.Edu.DAL.Entities
{
    public class TeamWithMemeber
    {      
        public Guid TeamsId { get; set; }       
        public string Name { get; set; }
        public string Description { get; set; }
        public string ExternalId { get; set; }
        public string InternalId { get; set; }
        public string JoinCode { get; set; }
        public string JoinUrl { get; set; }
        public bool IsMembershipLimitedToOwners { get; set; }
        public string TeamType { get; set; }
        public Guid MemberId { get; set; }
        public string MemberType { get; set; }
        public string UserPrincipalName { get; set; }
    }
}
