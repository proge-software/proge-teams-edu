using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.DAL.Entities
{
    public class TeamMember
    {
        public Guid TeamId { get; set; }
        public Guid MemberId { get; set; }
        public Member Member { get; set; }
        public Team Team { get; set; }
        public MemberType MemberType { get; set; }
    }

    public enum MemberType
    {
        Member = 0,
        Owner = 1,
    }
}
