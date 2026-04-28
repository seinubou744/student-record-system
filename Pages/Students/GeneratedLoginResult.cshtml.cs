using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages_Students
{
    [Authorize(Roles = "Admin")]
    public class GeneratedLoginResultModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public GeneratedLoginResultModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Student Student { get; set; } = default!;

        [TempData]
        public string? GeneratedEmail { get; set; }

        [TempData]
        public string? GeneratedPassword { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var student = await _context.Students
                .Include(s => s.ClassRoom)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(GeneratedEmail) || string.IsNullOrEmpty(GeneratedPassword))
            {
                return RedirectToPage("./Index");
            }

            Student = student;
            return Page();
        }
    }
}