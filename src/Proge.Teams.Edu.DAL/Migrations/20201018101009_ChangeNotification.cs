using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Proge.Teams.Edu.DAL.Migrations
{
    public partial class ChangeNotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChangeNotification",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ChangeType = table.Column<string>(nullable: false),
                    ODataId = table.Column<string>(nullable: false),
                    DeletedOn = table.Column<DateTime>(nullable: true),
                    RawJson = table.Column<string>(maxLength: 2147483647, nullable: true),
                    SubscriptionId = table.Column<Guid>(nullable: true),
                    TenantId = table.Column<Guid>(nullable: true),
                    SubscriptionExpirationDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", nullable: true),
                    Resource = table.Column<string>(maxLength: 400, nullable: true),
                    ODataType = table.Column<string>(maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeNotification", x => new { x.Id, x.ODataId, x.ChangeType });
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChangeNotification_ChangeType",
                table: "ChangeNotification",
                column: "ChangeType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChangeNotification");
        }
    }
}
