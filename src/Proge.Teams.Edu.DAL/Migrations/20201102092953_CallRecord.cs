using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Proge.Teams.Edu.DAL.Migrations
{
    public partial class CallRecord : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CallRecord",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Type = table.Column<string>(maxLength: 50, nullable: true),
                    JoinWebUrl = table.Column<string>(nullable: true),
                    CallDescription = table.Column<string>(maxLength: 200, nullable: true),
                    StartDateTime = table.Column<DateTimeOffset>(nullable: true),
                    EndDateTime = table.Column<DateTimeOffset>(nullable: true),
                    Modalities = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallRecord", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CallSession",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    StartDateTime = table.Column<DateTimeOffset>(nullable: true),
                    EndDateTime = table.Column<DateTimeOffset>(nullable: true),
                    UserPlatform = table.Column<string>(maxLength: 100, nullable: true),
                    UserProductFamily = table.Column<string>(maxLength: 100, nullable: true),
                    CallRecordId = table.Column<Guid>(nullable: false),
                    CallUserId = table.Column<Guid>(nullable: false),
                    CallUserRole = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CallSession_CallRecord_CallRecordId",
                        column: x => x.CallRecordId,
                        principalTable: "CallRecord",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CallUser",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserRole = table.Column<string>(maxLength: 50, nullable: false),
                    CallRecordId = table.Column<Guid>(nullable: false),
                    DisplayName = table.Column<string>(maxLength: 400, nullable: true),
                    UserTenantId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallUser", x => new { x.Id, x.UserRole, x.CallRecordId });
                    table.ForeignKey(
                        name: "FK_CallUser_CallRecord_CallRecordId",
                        column: x => x.CallRecordId,
                        principalTable: "CallRecord",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CallSessionSegment",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    StartDateTime = table.Column<DateTimeOffset>(nullable: true),
                    EndDateTime = table.Column<DateTimeOffset>(nullable: true),
                    UserPlatform = table.Column<string>(maxLength: 100, nullable: true),
                    UserProductFamily = table.Column<string>(maxLength: 100, nullable: true),
                    CallSessionId = table.Column<Guid>(nullable: false),
                    CallUserId = table.Column<Guid>(nullable: false),
                    CallUserRole = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallSessionSegment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CallSessionSegment_CallSession_CallSessionId",
                        column: x => x.CallSessionId,
                        principalTable: "CallSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CallRecord_Id",
                table: "CallRecord",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_CallRecord_JoinWebUrl",
                table: "CallRecord",
                column: "JoinWebUrl");

            migrationBuilder.CreateIndex(
                name: "IX_CallRecord_Type",
                table: "CallRecord",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_CallSession_CallRecordId",
                table: "CallSession",
                column: "CallRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_CallSession_Id",
                table: "CallSession",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_CallSessionSegment_CallSessionId",
                table: "CallSessionSegment",
                column: "CallSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_CallSessionSegment_Id",
                table: "CallSessionSegment",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_CallUser_CallRecordId",
                table: "CallUser",
                column: "CallRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_CallUser_DisplayName",
                table: "CallUser",
                column: "DisplayName");

            migrationBuilder.CreateIndex(
                name: "IX_CallUser_UserRole",
                table: "CallUser",
                column: "UserRole");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CallSessionSegment");

            migrationBuilder.DropTable(
                name: "CallUser");

            migrationBuilder.DropTable(
                name: "CallSession");

            migrationBuilder.DropTable(
                name: "CallRecord");
        }
    }
}
