using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentRecordSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddQuranFieldsToStudentLearningRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Portion",
                table: "StudentLearningRecords",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "FromAyah",
                table: "StudentLearningRecords",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SurahName",
                table: "StudentLearningRecords",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ToAyah",
                table: "StudentLearningRecords",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromAyah",
                table: "StudentLearningRecords");

            migrationBuilder.DropColumn(
                name: "SurahName",
                table: "StudentLearningRecords");

            migrationBuilder.DropColumn(
                name: "ToAyah",
                table: "StudentLearningRecords");

            migrationBuilder.AlterColumn<string>(
                name: "Portion",
                table: "StudentLearningRecords",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
