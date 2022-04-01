using Microsoft.EntityFrameworkCore.Migrations;

namespace Proge.Teams.Edu.DAL.Migrations
{
    public partial class EnrichTeamsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdCod",
                table: "Teams",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdDesc",
                table: "Teams",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnnoOfferta",
                table: "Teams",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnnoOrdinamento",
                table: "Teams",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CdsCod",
                table: "Teams",
                maxLength: 450,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdCod",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "AdDesc",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "AnnoOfferta",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "AnnoOrdinamento",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "CdsCod",
                table: "Teams");
        }
    }
}
