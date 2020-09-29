using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.DAL.MigrationsScripts
{
    public class TeamWithMemeber : ICustomMigration
    {
        public static TeamWithMemeber Default => new TeamWithMemeber();

        public string Up()
        {
            return @"
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER VIEW [dbo].[TeamsWithMemeber]
AS 
SELECT t.TeamsId, t.Name, t.Description, t.ExternalId, t.InternalId, t.JoinCode, t.JoinUrl,t.IsMembershipLimitedToOwners,t.TeamType, m.MemberId, tm.MemberType, m.UserPrincipalName
  FROM [dbo].Teams t
  inner join TeamMembers tm on t.TeamsId = tm.TeamId
  inner join Members m on tm.MemberId = m.MemberId  
  --order by t.TeamsId
GO

";
        }

        public string Down()
        {
            return @"DROP VIEW IF EXISTS [dbo].[TeamsWithMemeber]";
        }
    }
}
