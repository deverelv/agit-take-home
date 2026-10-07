using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AccessRequestHub.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ManagerEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Email);
                    table.ForeignKey(
                        name: "FK_users_users_ManagerEmail",
                        column: x => x.ManagerEmail,
                        principalTable: "users",
                        principalColumn: "Email");
                });

            migrationBuilder.CreateTable(
                name: "applications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SystemOwnerEmail = table.Column<string>(type: "nvarchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_applications_users_SystemOwnerEmail",
                        column: x => x.SystemOwnerEmail,
                        principalTable: "users",
                        principalColumn: "Email",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "access_requests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientRequestId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RequesterEmail = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    Environment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccessLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Justification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PolicyVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_access_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_access_requests_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_access_requests_users_RequesterEmail",
                        column: x => x.RequesterEmail,
                        principalTable: "users",
                        principalColumn: "Email",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    ActorEmail = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_logs_access_requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "access_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_audit_logs_users_ActorEmail",
                        column: x => x.ActorEmail,
                        principalTable: "users",
                        principalColumn: "Email",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "Email", "ManagerEmail", "Name", "Role" },
                values: new object[,]
                {
                    { "bob@example.local", null, "Bob", "Manager" },
                    { "carol@example.local", null, "Carol", "SystemOwner" },
                    { "dana@example.local", null, "Dana", "SystemOwner" },
                    { "erin@example.local", null, "Erin", "Auditor" }
                });

            migrationBuilder.InsertData(
                table: "applications",
                columns: new[] { "Id", "Name", "SystemOwnerEmail" },
                values: new object[,]
                {
                    { 1, "CRM", "carol@example.local" },
                    { 2, "Finance Portal", "dana@example.local" }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "Email", "ManagerEmail", "Name", "Role" },
                values: new object[] { "alice@example.local", "bob@example.local", "Alice", "Requester" });

            migrationBuilder.CreateIndex(
                name: "IX_access_requests_ApplicationId",
                table: "access_requests",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_access_requests_ClientRequestId",
                table: "access_requests",
                column: "ClientRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_access_requests_RequesterEmail",
                table: "access_requests",
                column: "RequesterEmail");

            migrationBuilder.CreateIndex(
                name: "IX_applications_SystemOwnerEmail",
                table: "applications",
                column: "SystemOwnerEmail");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_ActorEmail",
                table: "audit_logs",
                column: "ActorEmail");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_RequestId",
                table: "audit_logs",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_users_ManagerEmail",
                table: "users",
                column: "ManagerEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "access_requests");

            migrationBuilder.DropTable(
                name: "applications");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
