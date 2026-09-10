namespace StudentRecordSystem.Models
{
    public class StudentTestRecord
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Student? Student { get; set; }

        public int ClassRoomId { get; set; }
        public ClassRoom? ClassRoom { get; set; }

        public LearningRecordType TestType { get; set; }
        public DateTime TestDate { get; set; }

        // Quran range
        public string? FromSurah { get; set; }
        public int? FromAyah { get; set; }
        public string? ToSurah { get; set; }
        public int? ToAyah { get; set; }

        // Mutoon range
        public string? MatnName { get; set; }
        public string? FromPortion { get; set; }
        public string? ToPortion { get; set; }

        public decimal Marks { get; set; }
        public string? Remarks { get; set; }

        public string TeacherUserId { get; set; } = string.Empty;
        public ApplicationUser? TeacherUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}