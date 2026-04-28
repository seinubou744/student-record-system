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
    public class DetailsModel : PageModel
    {
        private readonly StudentRecordSystem.Data.ApplicationDbContext _context;

        public DetailsModel(StudentRecordSystem.Data.ApplicationDbContext context)
        {
            _context = context;
        }

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
    }
}
