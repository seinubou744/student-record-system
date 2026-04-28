using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages_Students
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Student Student { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.ClassRoom)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            Student = student;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            var attendances = await _context.Attendances
                .Where(a => a.StudentId == student.Id)
                .ToListAsync();

            var quranRecords = await _context.QuranRecords
                .Where(q => q.StudentId == student.Id)
                .ToListAsync();

            var mutoonRecords = await _context.MutoonRecords
                .Where(m => m.StudentId == student.Id)
                .ToListAsync();

            if (attendances.Any())
            {
                _context.Attendances.RemoveRange(attendances);
            }

            if (quranRecords.Any())
            {
                _context.QuranRecords.RemoveRange(quranRecords);
            }

            if (mutoonRecords.Any())
            {
                _context.MutoonRecords.RemoveRange(mutoonRecords);
            }

            if (!string.IsNullOrEmpty(student.ApplicationUserId))
            {
                var user = await _userManager.FindByIdAsync(student.ApplicationUserId);
                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }

                student.ApplicationUserId = null;
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}