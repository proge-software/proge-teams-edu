using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Proge.Teams.Edu.DAL.Migrations
{
    public partial class AddExamTeamRequestTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExamTeamsRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    App_appelloId = table.Column<long>(nullable: true),
                    App_dataFineIscr = table.Column<DateTime>(nullable: true),
                    App_dataInizioApp = table.Column<DateTime>(nullable: true),
                    App_dataInizioIscr = table.Column<DateTime>(nullable: true),
                    App_cdsId = table.Column<long>(nullable: true),
                    App_cdsCod = table.Column<string>(maxLength: 50, nullable: true),
                    App_cdsDes = table.Column<string>(maxLength: 400, nullable: true),
                    TeamRequested = table.Column<bool>(nullable: false),
                    TeamRequestDate = table.Column<DateTime>(nullable: true),
                    TeamRequestUser = table.Column<string>(maxLength: 50, nullable: true),
                    TeamCreated = table.Column<bool>(nullable: false),
                    TeamCreationDate = table.Column<DateTime>(nullable: true),
                    TeamJoinUrl = table.Column<string>(maxLength: 450, nullable: true),
                    TeamId = table.Column<string>(maxLength: 50, nullable: true),
                    Owners = table.Column<string>(nullable: true),
                    Members = table.Column<string>(nullable: true),
                    InternalId = table.Column<string>(maxLength: 100, nullable: true),
                    IsMembershipLimitedToOwners = table.Column<bool>(nullable: false),
                    MailSent = table.Column<bool>(nullable: true),
                    Ins_aa_offerta = table.Column<string>(maxLength: 50, nullable: true),
                    Ins_cds_cod = table.Column<string>(maxLength: 50, nullable: true),
                    Ins_aa_ord = table.Column<string>(maxLength: 50, nullable: true),
                    Ins_pds_cod = table.Column<string>(maxLength: 50, nullable: true),
                    Ins_ad_cod = table.Column<string>(maxLength: 50, nullable: true),
                    ExternalId = table.Column<string>(maxLength: 50, nullable: true),
                    Title = table.Column<string>(nullable: true),
                    DipCod = table.Column<string>(maxLength: 150, nullable: true),
                    AdditionalDataString = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamTeamsRequest", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamTeamsRequest");
        }
    }
}
