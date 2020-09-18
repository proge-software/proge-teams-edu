using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Proge.Teams.Edu.DAL.Migrations
{
    public partial class TenantId_BaseEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamMembers_Members_MemberId",
                table: "TeamMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Members",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "AzureAdId",
                table: "Members");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Teams",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Members",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                table: "Members",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Members",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Members",
                table: "Members",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMembers_Members_MemberId",
                table: "TeamMembers",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamMembers_Members_MemberId",
                table: "TeamMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Members",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Members");

            migrationBuilder.AddColumn<Guid>(
                name: "AzureAdId",
                table: "Members",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Members",
                table: "Members",
                column: "AzureAdId");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMembers_Members_MemberId",
                table: "TeamMembers",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "AzureAdId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
