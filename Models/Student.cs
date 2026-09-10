using System.ComponentModel.DataAnnotations;

namespace StudentRecordSystem.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string AdmissionNo { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public int SectionId { get; set; }
        public Section? Section { get; set; }

        [Required]
        public int ClassRoomId { get; set; }
        public ClassRoom? ClassRoom { get; set; }

        public string? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }

        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<QuranRecord> QuranRecords { get; set; } = new List<QuranRecord>();
        public ICollection<MutoonRecord> MutoonRecords { get; set; } = new List<MutoonRecord>();
    }
}