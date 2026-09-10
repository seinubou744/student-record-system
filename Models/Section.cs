using System.ComponentModel.DataAnnotations;

namespace StudentRecordSystem.Models
{
    public class Section
    {
        public int Id { get; set; }

        [Required]
        public int ClassRoomId { get; set; }
        public ClassRoom? ClassRoom { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        public ICollection<Student> Students { get; set; } = new List<Student>();
        public ICollection<TimetableEntry> TimetableEntries { get; set; } = new List<TimetableEntry>();
    }
}