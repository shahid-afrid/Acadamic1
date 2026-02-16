using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamPro1.Migrations
{
    /// <inheritdoc />
    public partial class AddFacultyIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FacultyId",
                table: "Faculties",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FacultyId",
                table: "Faculties");
        }
    }
}



