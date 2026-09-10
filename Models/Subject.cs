using System.ComponentModel.DataAnnotations;

namespace StudentRecordSystem.Models
{
    public class Subject
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int ClassRoomId { get; set; }

        public ClassRoom? ClassRoom { get; set; }

        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<TimetableEntry> TimetableEntries { get; set; } = new List<TimetableEntry>();
    }
}