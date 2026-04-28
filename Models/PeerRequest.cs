using System.ComponentModel.DataAnnotations;

namespace StudentRecordSystem.Models
{
    public class PeerRequest
    {
        public int Id { get; set; }

        [Required]
        public int SenderStudentId { get; set; }

        [Required]
        public int ReceiverStudentId { get; set; }

        [Required]
        [StringLength(20)]
        public string RequestType { get; set; } = ""; // Quran or Mutoon

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected

        [StringLength(500)]
        public string? Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Student? SenderStudent { get; set; }
        public Student? ReceiverStudent { get; set; }
    }
}