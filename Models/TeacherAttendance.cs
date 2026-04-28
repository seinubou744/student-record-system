using System.ComponentModel.DataAnnotations;

namespace StudentRecordSystem.Models
{
    public class TeacherAttendance
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        [Required]
        public int ClassRoomId { get; set; }
        public ClassRoom? ClassRoom { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime AttendanceDate { get; set; } = DateTime.Today;

        [Required]
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

        public string? TeacherUserId { get; set; }
        public ApplicationUser? TeacherUser { get; set; }

        [StringLength(300)]
        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}