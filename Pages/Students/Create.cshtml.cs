using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace StudentRecordSystem.Pages_Students
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
        ViewData["ClassRoomId"] = new SelectList(_context.ClassRooms, "Id", "Name");
            return Page();
        }

        [BindProperty]
        public Student Student { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Students.Add(Student);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
