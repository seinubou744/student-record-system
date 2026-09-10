using Microsoft.AspNetCore.Identity;

namespace StudentRecordSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public int? StudentId { get; set; }

        public Student? Student { get; set; }

        public ICollection<TimetableEntry> TeachingTimetableEntries { get; set; } = new List<TimetableEntry>();
    }
}