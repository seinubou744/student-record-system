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
        [Display(Name = "Portion Reached")]
        public string Portion { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Record Date")]
        public DateTime RecordDate { get; set; } = DateTime.Today;

        [StringLength(300)]
        public string? Remarks { get; set; }
    }
}