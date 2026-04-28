using Microsoft.AspNetCore.Identity;

namespace StudentRecordSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int? StudentId { get; set; }
        public Student? Student { get; set; }

        public string FullName { get; set; } = string.Empty;
    }
}