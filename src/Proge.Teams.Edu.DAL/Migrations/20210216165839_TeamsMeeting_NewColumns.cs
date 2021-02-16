using Microsoft.EntityFrameworkCore.Migrations;

namespace Proge.Teams.Edu.DAL.Migrations
{
    public partial class TeamsMeeting_NewColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomAttribute",
                table: "TeamsMeeting",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MeetingHierarchy",
                table: "TeamsMeeting",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MeetingType",
                table: "TeamsMeeting",
                maxLength: 255,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomAttribute",
                table: "TeamsMeeting");

            migrationBuilder.DropColumn(
                name: "MeetingHierarchy",
                table: "TeamsMeeting");

            migrationBuilder.DropColumn(
                name: "MeetingType",
                table: "TeamsMeeting");
        }
    }
}
