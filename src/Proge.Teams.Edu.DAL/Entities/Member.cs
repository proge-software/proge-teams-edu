using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.DAL.Entities
{
    public class Member : BaseEntity
    {
        public Guid MemberId { get; set; }
        public Guid? TenantId { get; set; }
        public string UserPrincipalName { get; set; }
        public string JobTitle { get; set; }
        public string OfficeLocation { get; set; }
        public string DisplayName { get; set; }
        public string Mail { get; set; }
      
        public ICollection<TeamMember> TeamsUsers { get; set; }
    }

   
}

