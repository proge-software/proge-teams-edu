using Microsoft.EntityFrameworkCore.Migrations;

namespace Proge.Teams.Edu.DAL.Migrations
{
    public partial class TeamsMeeting_MeetingsDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MeetingExtendedAttribute",
                table: "TeamsMeeting",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MeetingExtendedName",
                table: "TeamsMeeting",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MeetingIdPrimary",
                table: "TeamsMeeting",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MeetingIdSecondary",
                table: "TeamsMeeting",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerExtended",
                table: "TeamsMeeting",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerUpn",
                table: "TeamsMeeting",
                maxLength: 255,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MeetingExtendedAttribute",
                table: "TeamsMeeting");

            migrationBuilder.DropColumn(
                name: "MeetingExtendedName",
                table: "TeamsMeeting");

            migrationBuilder.DropColumn(
                name: "MeetingIdPrimary",
                table: "TeamsMeeting");

            migrationBuilder.DropColumn(
                name: "MeetingIdSecondary",
                table: "TeamsMeeting");

            migrationBuilder.DropColumn(
                name: "OwnerExtended",
                table: "TeamsMeeting");

            migrationBuilder.DropColumn(
                name: "OwnerUpn",
                table: "TeamsMeeting");
        }
    }
}
