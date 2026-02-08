using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamPro1.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamMeetingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamRequests_Students_ReceiverId",
                table: "TeamRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamRequests_Students_SenderId",
                table: "TeamRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Students_Student1Id",
                table: "Teams");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Students_Student2Id",
                table: "Teams");

            migrationBuilder.CreateTable(
                name: "TeamMeetings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    MeetingNumber = table.Column<int>(type: "int", nullable: false),
                    MeetingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletionPercentage = table.Column<int>(type: "int", nullable: false),
                    ProofUploads = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FacultyReview = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamMeetings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamMeetings_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeamMeetings_TeamId",
                table: "TeamMeetings",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamRequests_Students_ReceiverId",
                table: "TeamRequests",
                column: "ReceiverId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamRequests_Students_SenderId",
                table: "TeamRequests",
                column: "SenderId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Students_Student1Id",
                table: "Teams",
                column: "Student1Id",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Students_Student2Id",
                table: "Teams",
                column: "Student2Id",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamRequests_Students_ReceiverId",
                table: "TeamRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamRequests_Students_SenderId",
                table: "TeamRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Students_Student1Id",
                table: "Teams");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Students_Student2Id",
                table: "Teams");

            migrationBuilder.DropTable(
                name: "TeamMeetings");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamRequests_Students_ReceiverId",
                table: "TeamRequests",
                column: "ReceiverId",
                principalTable: "Students",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamRequests_Students_SenderId",
                table: "TeamRequests",
                column: "SenderId",
                principalTable: "Students",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Students_Student1Id",
                table: "Teams",
                column: "Student1Id",
                principalTable: "Students",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Students_Student2Id",
                table: "Teams",
                column: "Student2Id",
                principalTable: "Students",
                principalColumn: "Id");
        }
    }
}
