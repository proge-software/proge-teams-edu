using Microsoft.EntityFrameworkCore.Migrations;
using Proge.Teams.Edu.DAL.MigrationsScripts;
using System.Collections.Generic;

namespace Proge.Teams.Edu.DAL.Migrations
{
    public partial class LatestLogsView : Migration
    {
        private List<ICustomMigration> Migrations { get; set; }
        public LatestLogsView()
        {
            Migrations = new List<ICustomMigration>();
            Migrations.Add(TeamWithMemeber.Default);
            Migrations.Add(LatestLog.Default);
        }
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            Migrations.ForEach(a => migrationBuilder.Sql(a.Up()));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            Migrations.Reverse();
            Migrations.ForEach(a => migrationBuilder.Sql(a.Down()));
        }
    }
}
