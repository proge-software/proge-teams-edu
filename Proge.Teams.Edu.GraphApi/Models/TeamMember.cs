using Proge.Teams.Edu.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.GraphApi.Models
{
    public class TeamMember : ITeamMember
    {
        public string AzureAdId { get; set; }
        public string UserPrincipalName { get; set; }
        public string Name { get; set; }
        public string SecondName { get; set; }
        public string Mail { get; set; }
    }
}
