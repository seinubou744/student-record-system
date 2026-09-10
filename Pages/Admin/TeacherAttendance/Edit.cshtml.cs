using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages.Admin.TeacherAttendance
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
        public StudentRecordSystem.Models.TeacherAttendance TeacherAttendanceRecord { get; set; } = default!;

        public SelectList StudentList { get; set; } = default!;
        public SelectList ClassList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var record = await _context.TeacherAttendances
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id.Value);

            if (record == null)
                return NotFound();

            TeacherAttendanceRecord = record;
            await LoadListsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadListsAsync();
                return Page();
            }

            var recordToUpdate = await _context.TeacherAttendances
                .FirstOrDefaultAsync(t => t.Id == TeacherAttendanceRecord.Id);

            if (recordToUpdate == null)
                return NotFound();

            var studentExists = await _context.Students
                .AsNoTracking()
                .AnyAsync(s => s.Id == TeacherAttendanceRecord.StudentId);

            if (!studentExists)
            {
                ModelState.AddModelError(string.Empty, "Selected student does not exist.");
                await LoadListsAsync();
                return Page();
            }

            var classExists = await _context.ClassRooms
                .AsNoTracking()
                .AnyAsync(c => c.Id == TeacherAttendanceRecord.ClassRoomId);

            if (!classExists)
            {
                ModelState.AddModelError(string.Empty, "Selected class does not exist.");
                await LoadListsAsync();
                return Page();
            }

            recordToUpdate.StudentId = TeacherAttendanceRecord.StudentId;
            recordToUpdate.ClassRoomId = TeacherAttendanceRecord.ClassRoomId;
            recordToUpdate.AttendanceDate = TeacherAttendanceRecord.AttendanceDate.Date;
            recordToUpdate.Status = TeacherAttendanceRecord.Status;
            recordToUpdate.Remarks = string.IsNullOrWhiteSpace(TeacherAttendanceRecord.Remarks)
                ? null
                : TeacherAttendanceRecord.Remarks.Trim();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _context.TeacherAttendances
                    .AsNoTracking()
                    .AnyAsync(t => t.Id == TeacherAttendanceRecord.Id);

                if (!exists)
                    return NotFound();

                throw;
            }

            return RedirectToPage("./Index");
        }

        private async Task LoadListsAsync()
        {
            StudentList = new SelectList(
                await _context.Students
                    .AsNoTracking()
                    .OrderBy(s => s.FullName)
                    .ToListAsync(),
                "Id",
                "FullName");

            ClassList = new SelectList(
                await _context.ClassRooms
                    .AsNoTracking()
                    .OrderBy(c => c.Name)
                    .ToListAsync(),
                "Id",
                "Name");
        }
    }
}