using Microsoft.EntityFrameworkCore.Migrations;

namespace Proge.Teams.Edu.DAL.Migrations
{
    public partial class TeamsMeetingTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeamsMeeting",
                columns: table => new
                {
                    JoinUrl = table.Column<string>(maxLength: 450, nullable: false),
                    MeetingName = table.Column<string>(maxLength: 200, nullable: true),
                    MeetingId = table.Column<string>(maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamsMeeting", x => x.JoinUrl);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamsMeeting");
        }
    }
}
