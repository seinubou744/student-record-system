using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students => Set<Student>();
        public DbSet<ClassRoom> ClassRooms => Set<ClassRoom>();
        public DbSet<Subject> Subjects => Set<Subject>();
        public DbSet<Attendance> Attendances => Set<Attendance>();
        public DbSet<QuranRecord> QuranRecords => Set<QuranRecord>();
        public DbSet<MutoonRecord> MutoonRecords => Set<MutoonRecord>();
        public DbSet<PeerRequest> PeerRequests { get; set; }
        public DbSet<TeacherClass> TeacherClasses { get; set; }
        public DbSet<StudentAttendance> StudentAttendances { get; set; }
        public DbSet<StudentLearningRecord> StudentLearningRecords { get; set; }
        public DbSet<StudentTestRecord> StudentTestRecords { get; set; }
        public DbSet<TeacherAttendance> TeacherAttendances { get; set; }
        public DbSet<Section> Sections => Set<Section>();
        public DbSet<TimetableEntry> TimetableEntries => Set<TimetableEntry>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Student>()
                .HasIndex(s => s.AdmissionNo)
                .IsUnique();

            modelBuilder.Entity<Attendance>()
                .HasIndex(a => new { a.StudentId, a.SubjectId, a.AttendanceDate })
                .IsUnique();

            modelBuilder.Entity<Student>()
                .HasOne(s => s.ClassRoom)
                .WithMany(c => c.Students)
                .HasForeignKey(s => s.ClassRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Subject>()
                .HasOne(s => s.ClassRoom)
                .WithMany(c => c.Subjects)
                .HasForeignKey(s => s.ClassRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Student)
                .WithMany(s => s.Attendances)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Subject)
                .WithMany(s => s.Attendances)
                .HasForeignKey(a => a.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QuranRecord>()
                .HasOne(q => q.Student)
                .WithMany(s => s.QuranRecords)
                .HasForeignKey(q => q.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MutoonRecord>()
                .HasOne(m => m.Student)
                .WithMany(s => s.MutoonRecords)
                .HasForeignKey(m => m.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Student>()
                .HasOne(s => s.ApplicationUser)
                .WithOne(u => u.Student)
                .HasForeignKey<Student>(s => s.ApplicationUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PeerRequest>()
                .HasOne(p => p.SenderStudent)
                .WithMany()
                .HasForeignKey(p => p.SenderStudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PeerRequest>()
                .HasOne(p => p.ReceiverStudent)
                .WithMany()
                .HasForeignKey(p => p.ReceiverStudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeacherClass>()
                .HasOne(tc => tc.TeacherUser)
                .WithMany()
                .HasForeignKey(tc => tc.TeacherUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeacherClass>()
                .HasOne(tc => tc.ClassRoom)
                .WithMany()
                .HasForeignKey(tc => tc.ClassRoomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentAttendance>()
                .HasOne(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentAttendance>()
                .HasOne(a => a.ClassRoom)
                .WithMany()
                .HasForeignKey(a => a.ClassRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentAttendance>()
                .HasOne(a => a.TeacherUser)
                .WithMany()
                .HasForeignKey(a => a.TeacherUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentLearningRecord>()
                .HasOne(r => r.Student)
                .WithMany()
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentLearningRecord>()
                .HasOne(r => r.ClassRoom)
                .WithMany()
                .HasForeignKey(r => r.ClassRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentLearningRecord>()
                .HasOne(r => r.TeacherUser)
                .WithMany()
                .HasForeignKey(r => r.TeacherUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentTestRecord>()
                .HasOne(t => t.Student)
                .WithMany()
                .HasForeignKey(t => t.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentTestRecord>()
                .HasOne(t => t.ClassRoom)
                .WithMany()
                .HasForeignKey(t => t.ClassRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentTestRecord>()
                .HasOne(t => t.TeacherUser)
                .WithMany()
                .HasForeignKey(t => t.TeacherUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeacherAttendance>()
                .HasOne(t => t.Student)
                .WithMany()
                .HasForeignKey(t => t.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeacherAttendance>()
                .HasOne(t => t.ClassRoom)
                .WithMany()
                .HasForeignKey(t => t.ClassRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeacherAttendance>()
                .HasOne(t => t.TeacherUser)
                .WithMany()
                .HasForeignKey(t => t.TeacherUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeacherClass>()
                .HasOne(tc => tc.TeacherUser)
                .WithMany()
                .HasForeignKey(tc => tc.TeacherUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeacherClass>()
                .HasOne(tc => tc.ClassRoom)
                .WithMany()
                .HasForeignKey(tc => tc.ClassRoomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Section>()
                .HasIndex(s => new { s.ClassRoomId, s.Name })
                .IsUnique();

            modelBuilder.Entity<Student>()
                .HasOne(s => s.Section)
                .WithMany(sec => sec.Students)
                .HasForeignKey(s => s.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Section>()
                .HasOne(s => s.ClassRoom)
                .WithMany(c => c.Sections)
                .HasForeignKey(s => s.ClassRoomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TimetableEntry>()
                .HasOne(t => t.ClassRoom)
                .WithMany(c => c.TimetableEntries)
                .HasForeignKey(t => t.ClassRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TimetableEntry>()
                .HasOne(t => t.Section)
                .WithMany(s => s.TimetableEntries)
                .HasForeignKey(t => t.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TimetableEntry>()
                .HasOne(t => t.Subject)
                .WithMany(s => s.TimetableEntries)
                .HasForeignKey(t => t.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TimetableEntry>()
                .HasOne(t => t.Teacher)
                .WithMany(u => u.TeachingTimetableEntries)
                .HasForeignKey(t => t.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TimetableEntry>()
                .HasIndex(t => new
                {
                    t.ClassRoomId,
                    t.SectionId,
                    t.DayOfWeek,
                    t.StartTime,
                    t.EndTime,
                    t.AcademicYear
                });

            modelBuilder.Entity<TimetableEntry>()
                .HasIndex(t => new
                {
                    t.TeacherId,
                    t.DayOfWeek,
                    t.StartTime,
                    t.EndTime,
                    t.AcademicYear
                });

        }
    }
}