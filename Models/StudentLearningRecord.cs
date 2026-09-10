namespace StudentRecordSystem.Models
{
    public class StudentLearningRecord
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Student? Student { get; set; }

        public int ClassRoomId { get; set; }
        public ClassRoom? ClassRoom { get; set; }

        public LearningRecordType RecordType { get; set; }
        public DateTime RecordDate { get; set; }

        public string? SurahName { get; set; }
        public int? FromAyah { get; set; }
        public int? ToAyah { get; set; }

        public string? MatnName { get; set; }
        public string? FromPortion { get; set; }
        public string? ToPortion { get; set; }

        public string? Remarks { get; set; }

        public string TeacherUserId { get; set; } = string.Empty;
        public ApplicationUser? TeacherUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}