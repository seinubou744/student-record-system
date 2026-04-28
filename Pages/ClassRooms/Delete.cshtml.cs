using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages_ClassRooms
{
    public class DeleteModel : PageModel
    {
        private readonly StudentRecordSystem.Data.ApplicationDbContext _context;

        public DeleteModel(StudentRecordSystem.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ClassRoom ClassRoom { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classroom = await _context.ClassRooms.FirstOrDefaultAsync(m => m.Id == id);

            if (classroom is not null)
            {
                ClassRoom = classroom;

                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classroom = await _context.ClassRooms.FindAsync(id);
            if (classroom != null)
            {
                ClassRoom = classroom;
                _context.ClassRooms.Remove(ClassRoom);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
