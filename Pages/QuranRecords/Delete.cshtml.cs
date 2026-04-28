using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages_QuranRecords
{
    public class DeleteModel : PageModel
    {
        private readonly StudentRecordSystem.Data.ApplicationDbContext _context;

        public DeleteModel(StudentRecordSystem.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public QuranRecord QuranRecord { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quranrecord = await _context.QuranRecords.FirstOrDefaultAsync(m => m.Id == id);

            if (quranrecord is not null)
            {
                QuranRecord = quranrecord;

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

            var quranrecord = await _context.QuranRecords.FindAsync(id);
            if (quranrecord != null)
            {
                QuranRecord = quranrecord;
                _context.QuranRecords.Remove(QuranRecord);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
