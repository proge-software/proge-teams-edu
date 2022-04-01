using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proge.Teams.Edu.DAL.Migrations
{
    public partial class CallSession_CallUserIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CallSession_CallUserId",
                table: "CallSession",
                column: "CallUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CallSession_CallUserId",
                table: "CallSession");
        }
    }
}
