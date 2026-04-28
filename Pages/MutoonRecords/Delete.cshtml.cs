using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages_MutoonRecords
{
    public class DeleteModel : PageModel
    {
        private readonly StudentRecordSystem.Data.ApplicationDbContext _context;

        public DeleteModel(StudentRecordSystem.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public MutoonRecord MutoonRecord { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mutoonrecord = await _context.MutoonRecords.FirstOrDefaultAsync(m => m.Id == id);

            if (mutoonrecord is not null)
            {
                MutoonRecord = mutoonrecord;

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

            var mutoonrecord = await _context.MutoonRecords.FindAsync(id);
            if (mutoonrecord != null)
            {
                MutoonRecord = mutoonrecord;
                _context.MutoonRecords.Remove(MutoonRecord);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
