using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentRecordSystem.Migrations
{
    /// <inheritdoc />
    public partial class FixStudentLearningRecordFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Portion",
                table: "StudentLearningRecords",
                newName: "ToPortion");

            migrationBuilder.AddColumn<string>(
                name: "FromPortion",
                table: "StudentLearningRecords",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MatnName",
                table: "StudentLearningRecords",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromPortion",
                table: "StudentLearningRecords");

            migrationBuilder.DropColumn(
                name: "MatnName",
                table: "StudentLearningRecords");

            migrationBuilder.RenameColumn(
                name: "ToPortion",
                table: "StudentLearningRecords",
                newName: "Portion");
        }
    }
}
