using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentRecordSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddMatnNameToStudentTestRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MatnName",
                table: "StudentTestRecords",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MatnName",
                table: "StudentTestRecords");
        }
    }
}
