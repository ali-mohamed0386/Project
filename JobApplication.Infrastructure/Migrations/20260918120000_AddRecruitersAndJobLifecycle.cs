using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecruitersAndJobLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ------------------------------------------------------------------
            // 1) The new Recruiters table
            // ------------------------------------------------------------------
            // It must be created first, because the AddForeignKey on
            // Jobs.RecruiterId depends on it.
            migrationBuilder.CreateTable(
                name: "Recruiters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recruiters", x => x.Id);
                });

            // ------------------------------------------------------------------
            // 2) Authentication columns on Candidates
            // ------------------------------------------------------------------
            // defaultValue: "" is required — the column is NOT NULL, and SQL
            // Server needs a default value in order to add it to a table that
            // already contains rows.
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Candidates",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Candidates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            // ------------------------------------------------------------------
            // 3) Job lifecycle columns on Jobs
            // ------------------------------------------------------------------
            migrationBuilder.AddColumn<int>(
                name: "RecruiterId",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                table: "Jobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClosedBy",
                table: "Jobs",
                type: "int",
                nullable: true);

            // ------------------------------------------------------------------
            // 4) Cancellation column on JobCandidateApplications (soft delete)
            // ------------------------------------------------------------------
            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "JobCandidateApplications",
                type: "datetime2",
                nullable: true);

            // ------------------------------------------------------------------
            // 5) Replace the single-column index with the composite unique index
            // ------------------------------------------------------------------
            // The old index on CandidateId is now covered by the new composite
            // index (CandidateId is its leading column), so it is dropped to
            // avoid maintaining a redundant index on every INSERT/UPDATE.
            migrationBuilder.DropIndex(
                name: "IX_JobCandidateApplications_CandidateId",
                table: "JobCandidateApplications");

            // This is the real protection against duplicate applications —
            // not the check performed inside the service.
            migrationBuilder.CreateIndex(
                name: "IX_JobCandidateApplications_CandidateId_JobId",
                table: "JobCandidateApplications",
                columns: new[] { "CandidateId", "JobId" },
                unique: true);

            // ------------------------------------------------------------------
            // 6) The new indexes
            // ------------------------------------------------------------------
            migrationBuilder.CreateIndex(
                name: "IX_Candidates_Email",
                table: "Candidates",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recruiters_Email",
                table: "Recruiters",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_RecruiterId",
                table: "Jobs",
                column: "RecruiterId");

            // ------------------------------------------------------------------
            // 7) The foreign key
            // ------------------------------------------------------------------
            // Restrict rather than Cascade: deleting a recruiter must not take
            // their jobs (and every application submitted to them) down with it.
            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Recruiters_RecruiterId",
                table: "Jobs",
                column: "RecruiterId",
                principalTable: "Recruiters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The order is exactly reversed — the FK is dropped before the
            // table itself is removed.
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Recruiters_RecruiterId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Candidates_Email",
                table: "Candidates");

            migrationBuilder.DropIndex(
                name: "IX_Recruiters_Email",
                table: "Recruiters");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_RecruiterId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_JobCandidateApplications_CandidateId_JobId",
                table: "JobCandidateApplications");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "RecruiterId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ClosedBy",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "JobCandidateApplications");

            // Restore the old index so that Down returns the schema to exactly
            // its previous state.
            migrationBuilder.CreateIndex(
                name: "IX_JobCandidateApplications_CandidateId",
                table: "JobCandidateApplications",
                column: "CandidateId");

            migrationBuilder.DropTable(
                name: "Recruiters");
        }
    }
}
