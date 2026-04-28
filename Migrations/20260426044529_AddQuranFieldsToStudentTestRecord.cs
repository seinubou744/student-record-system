using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentRecordSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddQuranFieldsToStudentTestRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Portion",
                table: "StudentTestRecords",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "FromAyah",
                table: "StudentTestRecords",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SurahName",
                table: "StudentTestRecords",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ToAyah",
                table: "StudentTestRecords",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromAyah",
                table: "StudentTestRecords");

            migrationBuilder.DropColumn(
                name: "SurahName",
                table: "StudentTestRecords");

            migrationBuilder.DropColumn(
                name: "ToAyah",
                table: "StudentTestRecords");

            migrationBuilder.AlterColumn<string>(
                name: "Portion",
                table: "StudentTestRecords",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
