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
    public class IndexModel : PageModel
    {
        private readonly StudentRecordSystem.Data.ApplicationDbContext _context;

        public IndexModel(StudentRecordSystem.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<QuranRecord> QuranRecord { get;set; } = default!;

        public async Task OnGetAsync()
        {
            QuranRecord = await _context.QuranRecords
                .Include(q => q.Student).ToListAsync();
        }
    }
}
