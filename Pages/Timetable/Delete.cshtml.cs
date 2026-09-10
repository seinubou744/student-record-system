using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages.Timetable
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
        public TimetableEntry TimetableEntry { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var entry = await _context.TimetableEntries
                .Include(t => t.ClassRoom)
                .Include(t => t.Section)
                .Include(t => t.Subject)
                .Include(t => t.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (entry == null)
                return NotFound();

            TimetableEntry = entry;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var entry = await _context.TimetableEntries.FindAsync(id);

            if (entry != null)
            {
                _context.TimetableEntries.Remove(entry);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}