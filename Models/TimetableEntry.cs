using System.ComponentModel.DataAnnotations;

namespace StudentRecordSystem.Models
{
    public class TimetableEntry
    {
        public int Id { get; set; }

        [Required]
        public int ClassRoomId { get; set; }
        public ClassRoom? ClassRoom { get; set; }

        [Required]
        public int SectionId { get; set; }
        public Section? Section { get; set; }

        [Required]
        public int SubjectId { get; set; }
        public Subject? Subject { get; set; }

        [Required]
        public string TeacherId { get; set; } = string.Empty;
        public ApplicationUser? Teacher { get; set; }

        [Required]
        public WeekDay DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [StringLength(50)]
        public string? RoomNumber { get; set; }

        [Required]
        [StringLength(20)]
        public string AcademicYear { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}