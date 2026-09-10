using System.ComponentModel.DataAnnotations;

namespace StudentRecordSystem.Models
{
    public class ClassRoom
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public ICollection<Student> Students { get; set; } = new List<Student>();
        public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
        public ICollection<Section> Sections { get; set; } = new List<Section>();
        public ICollection<TimetableEntry> TimetableEntries { get; set; } = new List<TimetableEntry>();
    }
}