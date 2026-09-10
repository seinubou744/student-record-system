using System.ComponentModel.DataAnnotations;

namespace StudentRecordSystem.Models
{
    public class MutoonRecord
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Student")]
        public int StudentId { get; set; }

        public Student? Student { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Matn Name")]
        public string MatnName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        [Display(Name = "From Portion")]
        public string FromPortion { get; set; } = string.Empty;

        [StringLength(150)]
        [Display(Name = "To Portion")]
        public string? ToPortion { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Record Date")]
        public DateTime RecordDate { get; set; } = DateTime.Today;

        [StringLength(300)]
        public string? Remarks { get; set; }
    }
}