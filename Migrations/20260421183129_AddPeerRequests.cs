using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentRecordSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPeerRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PeerRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SenderStudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReceiverStudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    RequestType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeerRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeerRequests_Students_ReceiverStudentId",
                        column: x => x.ReceiverStudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeerRequests_Students_SenderStudentId",
                        column: x => x.SenderStudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PeerRequests_ReceiverStudentId",
                table: "PeerRequests",
                column: "ReceiverStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerRequests_SenderStudentId",
                table: "PeerRequests",
                column: "SenderStudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PeerRequests");
        }
    }
}
