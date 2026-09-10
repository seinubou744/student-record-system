using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentRecordSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMutoonRecordRangeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Portion",
                table: "MutoonRecords",
                newName: "FromPortion");

            migrationBuilder.AddColumn<string>(
                name: "ToPortion",
                table: "MutoonRecords",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ToPortion",
                table: "MutoonRecords");

            migrationBuilder.RenameColumn(
                name: "FromPortion",
                table: "MutoonRecords",
                newName: "Portion");
        }
    }
}
