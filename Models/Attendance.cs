using System.ComponentModel.DataAnnotations;

namespace StudentRecordSystem.Models
{
    public class Attendance
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        public Student? Student { get; set; }

        [Required]
        public int SubjectId { get; set; }

        public Subject? Subject { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime AttendanceDate { get; set; } = DateTime.Today;

        public bool IsPresent { get; set; }
    }
}