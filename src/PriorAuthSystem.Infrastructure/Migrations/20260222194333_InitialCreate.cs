using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriorAuthSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MemberId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InsurancePlanId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FaxNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Payers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PayerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PayerId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StandardResponseDays = table.Column<int>(type: "int", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FaxNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Providers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NPI = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Specialty = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OrganizationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FaxNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Providers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PriorAuthorizationRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IcdCode = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    IcdDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CptCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    CptDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CptRequiresPriorAuth = table.Column<bool>(type: "bit", nullable: false),
                    ClinicalNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ClinicalSupportingDocumentPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ClinicalDocumentedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClinicalDocumentedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RequiredResponseBy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriorAuthorizationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriorAuthorizationRequests_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PriorAuthorizationRequests_Payers_PayerId",
                        column: x => x.PayerId,
                        principalTable: "Payers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PriorAuthorizationRequests_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatusTransitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PriorAuthorizationRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ToStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TransitionedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    TransitionedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusTransitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatusTransitions_PriorAuthorizationRequests_PriorAuthorizationRequestId",
                        column: x => x.PriorAuthorizationRequestId,
                        principalTable: "PriorAuthorizationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payers_PayerId",
                table: "Payers",
                column: "PayerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriorAuthorizationRequests_PatientId",
                table: "PriorAuthorizationRequests",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PriorAuthorizationRequests_PayerId",
                table: "PriorAuthorizationRequests",
                column: "PayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PriorAuthorizationRequests_ProviderId",
                table: "PriorAuthorizationRequests",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_NPI",
                table: "Providers",
                column: "NPI",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatusTransitions_PriorAuthorizationRequestId",
                table: "StatusTransitions",
                column: "PriorAuthorizationRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StatusTransitions");

            migrationBuilder.DropTable(
                name: "PriorAuthorizationRequests");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropTable(
                name: "Payers");

            migrationBuilder.DropTable(
                name: "Providers");
        }
    }
}
