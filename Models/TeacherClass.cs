using System.ComponentModel.DataAnnotations;

namespace StudentRecordSystem.Models
{
    public class TeacherClass
    {
        public int Id { get; set; }

        [Required]
        public string TeacherUserId { get; set; } = string.Empty;
        public ApplicationUser? TeacherUser { get; set; }

        [Required]
        public int ClassRoomId { get; set; }
        public ClassRoom? ClassRoom { get; set; }
    }
}