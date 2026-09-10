using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Services
{
    public class TimetableService
    {
        private readonly ApplicationDbContext _context;

        public TimetableService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string?> ValidateEntryAsync(TimetableEntry entry, int? ignoreId = null)
        {
            if (entry.StartTime >= entry.EndTime)
            {
                return "Start time must be earlier than end time.";
            }

            bool teacherConflict = await _context.TimetableEntries
                .AnyAsync(t =>
                    t.Id != (ignoreId ?? 0) &&
                    t.TeacherId == entry.TeacherId &&
                    t.DayOfWeek == entry.DayOfWeek &&
                    t.AcademicYear == entry.AcademicYear &&
                    entry.StartTime < t.EndTime &&
                    entry.EndTime > t.StartTime);

            if (teacherConflict)
            {
                return "The selected teacher already has another class during this time.";
            }

            bool classConflict = await _context.TimetableEntries
                .AnyAsync(t =>
                    t.Id != (ignoreId ?? 0) &&
                    t.ClassRoomId == entry.ClassRoomId &&
                    t.SectionId == entry.SectionId &&
                    t.DayOfWeek == entry.DayOfWeek &&
                    t.AcademicYear == entry.AcademicYear &&
                    entry.StartTime < t.EndTime &&
                    entry.EndTime > t.StartTime);

            if (classConflict)
            {
                return "This class and section already have another subject scheduled during this time.";
            }

            return null;
        }
    }
}