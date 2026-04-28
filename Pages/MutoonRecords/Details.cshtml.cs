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
    public class DetailsModel : PageModel
    {
        private readonly StudentRecordSystem.Data.ApplicationDbContext _context;

        public DetailsModel(StudentRecordSystem.Data.ApplicationDbContext context)
        {
            _context = context;
        }

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
    }
}
