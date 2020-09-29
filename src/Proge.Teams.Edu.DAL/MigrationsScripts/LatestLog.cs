using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.DAL.MigrationsScripts
{
    public class LatestLog : ICustomMigration
    {
        public static LatestLog Default => new LatestLog();

        public string Up()
        {
            return @"
CREATE OR ALTER VIEW [dbo].[LatestLog]
AS
    SELECT top(10000)
      [Message]      
      ,[Level]
      ,[TimeStamp]
      ,[Exception]      
  FROM [dbo].[Logs]
  order by TimeStamp desc;
GO
";
        }

        public string Down()
        {
            return @"DROP VIEW IF EXISTS [dbo].[LatestLog]";
        }
    }
}
