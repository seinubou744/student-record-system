using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentRecordSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTeacherPortalChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentAttendances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassRoomId = table.Column<int>(type: "INTEGER", nullable: false),
                    AttendanceDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    TeacherUserId = table.Column<string>(type: "TEXT", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentAttendances_AspNetUsers_TeacherUserId",
                        column: x => x.TeacherUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentAttendances_ClassRooms_ClassRoomId",
                        column: x => x.ClassRoomId,
                        principalTable: "ClassRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentAttendances_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentLearningRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassRoomId = table.Column<int>(type: "INTEGER", nullable: false),
                    RecordType = table.Column<int>(type: "INTEGER", nullable: false),
                    RecordDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Portion = table.Column<string>(type: "TEXT", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", nullable: true),
                    TeacherUserId = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentLearningRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentLearningRecords_AspNetUsers_TeacherUserId",
                        column: x => x.TeacherUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentLearningRecords_ClassRooms_ClassRoomId",
                        column: x => x.ClassRoomId,
                        principalTable: "ClassRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentLearningRecords_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentTestRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassRoomId = table.Column<int>(type: "INTEGER", nullable: false),
                    TestType = table.Column<int>(type: "INTEGER", nullable: false),
                    TestDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Portion = table.Column<string>(type: "TEXT", nullable: false),
                    Marks = table.Column<decimal>(type: "TEXT", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", nullable: true),
                    TeacherUserId = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentTestRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentTestRecords_AspNetUsers_TeacherUserId",
                        column: x => x.TeacherUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentTestRecords_ClassRooms_ClassRoomId",
                        column: x => x.ClassRoomId,
                        principalTable: "ClassRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentTestRecords_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeacherAttendances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassRoomId = table.Column<int>(type: "INTEGER", nullable: false),
                    AttendanceDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    TeacherUserId = table.Column<string>(type: "TEXT", nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherAttendances_AspNetUsers_TeacherUserId",
                        column: x => x.TeacherUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherAttendances_ClassRooms_ClassRoomId",
                        column: x => x.ClassRoomId,
                        principalTable: "ClassRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherAttendances_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeacherClasses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TeacherUserId = table.Column<string>(type: "TEXT", nullable: false),
                    ClassRoomId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherClasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherClasses_AspNetUsers_TeacherUserId",
                        column: x => x.TeacherUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherClasses_ClassRooms_ClassRoomId",
                        column: x => x.ClassRoomId,
                        principalTable: "ClassRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentAttendances_ClassRoomId",
                table: "StudentAttendances",
                column: "ClassRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAttendances_StudentId",
                table: "StudentAttendances",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAttendances_TeacherUserId",
                table: "StudentAttendances",
                column: "TeacherUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentLearningRecords_ClassRoomId",
                table: "StudentLearningRecords",
                column: "ClassRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentLearningRecords_StudentId",
                table: "StudentLearningRecords",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentLearningRecords_TeacherUserId",
                table: "StudentLearningRecords",
                column: "TeacherUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTestRecords_ClassRoomId",
                table: "StudentTestRecords",
                column: "ClassRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTestRecords_StudentId",
                table: "StudentTestRecords",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTestRecords_TeacherUserId",
                table: "StudentTestRecords",
                column: "TeacherUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAttendances_ClassRoomId",
                table: "TeacherAttendances",
                column: "ClassRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAttendances_StudentId",
                table: "TeacherAttendances",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAttendances_TeacherUserId",
                table: "TeacherAttendances",
                column: "TeacherUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherClasses_ClassRoomId",
                table: "TeacherClasses",
                column: "ClassRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherClasses_TeacherUserId",
                table: "TeacherClasses",
                column: "TeacherUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentAttendances");

            migrationBuilder.DropTable(
                name: "StudentLearningRecords");

            migrationBuilder.DropTable(
                name: "StudentTestRecords");

            migrationBuilder.DropTable(
                name: "TeacherAttendances");

            migrationBuilder.DropTable(
                name: "TeacherClasses");
        }
    }
}
