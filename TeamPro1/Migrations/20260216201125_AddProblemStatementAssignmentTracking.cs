using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamPro1.Migrations
{
    /// <inheritdoc />
    public partial class AddProblemStatementAssignmentTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedAt",
                table: "ProblemStatementBanks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignedByFacultyId",
                table: "ProblemStatementBanks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignedToTeamId",
                table: "ProblemStatementBanks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAssigned",
                table: "ProblemStatementBanks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ProblemStatementBanks_AssignedByFacultyId",
                table: "ProblemStatementBanks",
                column: "AssignedByFacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProblemStatementBanks_AssignedToTeamId",
                table: "ProblemStatementBanks",
                column: "AssignedToTeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProblemStatementBanks_Faculties_AssignedByFacultyId",
                table: "ProblemStatementBanks",
                column: "AssignedByFacultyId",
                principalTable: "Faculties",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ProblemStatementBanks_Teams_AssignedToTeamId",
                table: "ProblemStatementBanks",
                column: "AssignedToTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProblemStatementBanks_Faculties_AssignedByFacultyId",
                table: "ProblemStatementBanks");

            migrationBuilder.DropForeignKey(
                name: "FK_ProblemStatementBanks_Teams_AssignedToTeamId",
                table: "ProblemStatementBanks");

            migrationBuilder.DropIndex(
                name: "IX_ProblemStatementBanks_AssignedByFacultyId",
                table: "ProblemStatementBanks");

            migrationBuilder.DropIndex(
                name: "IX_ProblemStatementBanks_AssignedToTeamId",
                table: "ProblemStatementBanks");

            migrationBuilder.DropColumn(
                name: "AssignedAt",
                table: "ProblemStatementBanks");

            migrationBuilder.DropColumn(
                name: "AssignedByFacultyId",
                table: "ProblemStatementBanks");

            migrationBuilder.DropColumn(
                name: "AssignedToTeamId",
                table: "ProblemStatementBanks");

            migrationBuilder.DropColumn(
                name: "IsAssigned",
                table: "ProblemStatementBanks");
        }
    }
}
