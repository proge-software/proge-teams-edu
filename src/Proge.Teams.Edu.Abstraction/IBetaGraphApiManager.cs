extern alias BetaLib;
using Beta = BetaLib.Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.Abstraction
{
    public interface IBetaGraphApiManager
    {
        Task ConnectAsApplication();
        Beta.Team DefaultTeamFactory(string groupId);
        Task<Beta.Team> GetTeam(string id);
    }
}
