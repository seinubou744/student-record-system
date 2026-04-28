// Models/StudentAttendance.cs
namespace StudentRecordSystem.Models
{
    public class StudentAttendance
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        public int ClassRoomId { get; set; }
        public ClassRoom? ClassRoom { get; set; }

        public DateTime AttendanceDate { get; set; }
        public AttendanceStatus Status { get; set; }

        public string TeacherUserId { get; set; } = string.Empty;
        public ApplicationUser? TeacherUser { get; set; }

        public string? Remarks { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}