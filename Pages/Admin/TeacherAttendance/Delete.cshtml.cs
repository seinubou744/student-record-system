using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;

namespace StudentRecordSystem.Pages.Admin.TeacherAttendance
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public StudentRecordSystem.Models.TeacherAttendance TeacherAttendanceRecord { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var record = await _context.TeacherAttendances
                .AsNoTracking()
                .Include(t => t.Student)
                .Include(t => t.ClassRoom)
                .Include(t => t.TeacherUser)
                .FirstOrDefaultAsync(t => t.Id == id.Value);

            if (record == null)
                return NotFound();

            TeacherAttendanceRecord = record;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var record = await _context.TeacherAttendances
                .FirstOrDefaultAsync(t => t.Id == id.Value);

            if (record == null)
                return RedirectToPage("./Index");

            _context.TeacherAttendances.Remove(record);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}