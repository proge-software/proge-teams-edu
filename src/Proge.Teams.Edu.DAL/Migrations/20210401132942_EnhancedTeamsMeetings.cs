using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Proge.Teams.Edu.DAL.Migrations
{
    public partial class EnhancedTeamsMeetings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CustomAttribute",
                table: "TeamsMeeting",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Attendees",
                table: "TeamsMeeting",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChatMessageId",
                table: "TeamsMeeting",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChatThreadId",
                table: "TeamsMeeting",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDateTime",
                table: "TeamsMeeting",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamsMeeting_MeetingId",
                table: "TeamsMeeting",
                column: "MeetingId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeamsMeeting_MeetingId",
                table: "TeamsMeeting");

            migrationBuilder.DropColumn(
                name: "Attendees",
                table: "TeamsMeeting");

            migrationBuilder.DropColumn(
                name: "ChatMessageId",
                table: "TeamsMeeting");

            migrationBuilder.DropColumn(
                name: "ChatThreadId",
                table: "TeamsMeeting");

            migrationBuilder.DropColumn(
                name: "CreationDateTime",
                table: "TeamsMeeting");

            migrationBuilder.AlterColumn<string>(
                name: "CustomAttribute",
                table: "TeamsMeeting",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
