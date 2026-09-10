using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages.Timetable
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public TimetableEntry TimetableEntry { get; set; } = default!;

        public SelectList ClassRooms { get; set; } = default!;
        public SelectList Sections { get; set; } = default!;
        public SelectList Subjects { get; set; } = default!;
        public SelectList Teachers { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var entry = await _context.TimetableEntries
                .FirstOrDefaultAsync(m => m.Id == id);

            if (entry == null)
                return NotFound();

            TimetableEntry = entry;
            await LoadSelectionsAsync(entry.ClassRoomId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadSelectionsAsync(TimetableEntry.ClassRoomId);
                return Page();
            }

            var entryInDb = await _context.TimetableEntries
                .FirstOrDefaultAsync(t => t.Id == TimetableEntry.Id);

            if (entryInDb == null)
                return NotFound();

            entryInDb.ClassRoomId = TimetableEntry.ClassRoomId;
            entryInDb.SectionId = TimetableEntry.SectionId;
            entryInDb.SubjectId = TimetableEntry.SubjectId;
            entryInDb.TeacherId = TimetableEntry.TeacherId;
            entryInDb.DayOfWeek = TimetableEntry.DayOfWeek;
            entryInDb.StartTime = TimetableEntry.StartTime;
            entryInDb.EndTime = TimetableEntry.EndTime;
            entryInDb.RoomNumber = TimetableEntry.RoomNumber;
            entryInDb.AcademicYear = TimetableEntry.AcademicYear;
            entryInDb.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }

        private async Task LoadSelectionsAsync(int? classRoomId = null)
        {
            var classRooms = await _context.ClassRooms
                .OrderBy(c => c.Name)
                .ToListAsync();

            var sectionsQuery = _context.Sections.AsQueryable();
            var subjectsQuery = _context.Subjects.AsQueryable();

            if (classRoomId.HasValue)
            {
                sectionsQuery = sectionsQuery.Where(s => s.ClassRoomId == classRoomId.Value);
                subjectsQuery = subjectsQuery.Where(s => s.ClassRoomId == classRoomId.Value);
            }

            var sections = await sectionsQuery.OrderBy(s => s.Name).ToListAsync();
            var subjects = await subjectsQuery.OrderBy(s => s.Name).ToListAsync();
            var teachers = await _context.Users.OrderBy(u => u.FullName).ToListAsync();

            ClassRooms = new SelectList(classRooms, "Id", "Name");
            Sections = new SelectList(sections, "Id", "Name");
            Subjects = new SelectList(subjects, "Id", "Name");
            Teachers = new SelectList(teachers, "Id", "FullName");
        }
    }
}