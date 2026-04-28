using System.ComponentModel.DataAnnotations;

namespace StudentRecordSystem.Models
{
    public class QuranRecord
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Student")]
        public int StudentId { get; set; }

        public Student? Student { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Surah Name")]
        public string SurahName { get; set; } = string.Empty;

        [Range(1, 604)]
        [Display(Name = "Page Number")]
        public int PageNumber { get; set; }

        [Range(1, 286)]
        [Display(Name = "From Ayah")]
        public int FromAyah { get; set; }

        [Range(1, 286)]
        [Display(Name = "To Ayah")]
        public int ToAyah { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Record Date")]
        public DateTime RecordDate { get; set; } = DateTime.Today;

        [StringLength(300)]
        public string? Remarks { get; set; }
    }
}