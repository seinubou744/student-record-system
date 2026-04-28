using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages.Attendances
{
    public class CreateModel : PageModel
    {
        private readonly StudentRecordSystem.Data.ApplicationDbContext _context;

        public CreateModel(StudentRecordSystem.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["StudentId"] = new SelectList(_context.Students, "Id", "AdmissionNo");
        ViewData["SubjectId"] = new SelectList(_context.Subjects, "Id", "Name");
            return Page();
        }

        [BindProperty]
        public Attendance Attendance { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attendances.Add(Attendance);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
