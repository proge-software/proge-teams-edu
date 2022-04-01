extern alias BetaLib;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Proge.Teams.Edu.Abstraction;
using Proge.Teams.Edu.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using Beta = BetaLib.Microsoft.Graph;
using Proge.Teams.Edu.TeamsDashboard;

namespace Proge.Teams.Edu.Function
{
    /// <summary>
    /// Custom implementation example of TeamsDataCollectorManager
    /// </summary>
    public class CustomUniSettings : UniSettings
    {

    }

    public class CustomCallFilters : CallFilters
    {

    }

    /// <summary>
    /// Custom implementation example of TeamsDataCollectorManager
    /// </summary>
    public class CustomTeamsDataCollectorManager : TeamsDataCollectorManager, ITeamsDataCollectorManager
    {
        public CustomTeamsDataCollectorManager(IOptions<CustomUniSettings> uniCfg,
            IOptions<CustomCallFilters> callFilter,
            IGraphApiManager betaGraphApi,
            ILogger<CustomTeamsDataCollectorManager> logger,
            ICallRecordRepository ichangenotrep,
            ITeamsMeetingRepository teamsMeetingRepository)
        : base(uniCfg, callFilter, betaGraphApi, logger, ichangenotrep, teamsMeetingRepository)
        { }


    }
}
