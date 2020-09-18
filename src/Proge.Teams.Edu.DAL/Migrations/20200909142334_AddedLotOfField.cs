using Microsoft.EntityFrameworkCore.Migrations;

namespace Proge.Teams.Edu.DAL.Migrations
{
    public partial class AddedLotOfField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Members_MemberType",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "MemberType",
                table: "Members");

            migrationBuilder.AddColumn<bool>(
                name: "IsMembershipLimitedToOwners",
                table: "Teams",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MemberType",
                table: "TeamMembers",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Members",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "Members",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mail",
                table: "Members",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfficeLocation",
                table: "Members",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_MemberType",
                table: "TeamMembers",
                column: "MemberType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeamMembers_MemberType",
                table: "TeamMembers");

            migrationBuilder.DropColumn(
                name: "IsMembershipLimitedToOwners",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "MemberType",
                table: "TeamMembers");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "Mail",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "OfficeLocation",
                table: "Members");

            migrationBuilder.AddColumn<int>(
                name: "MemberType",
                table: "Members",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Members_MemberType",
                table: "Members",
                column: "MemberType");
        }
    }
}
