using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages_MutoonRecords
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<MutoonRecord> MutoonRecord { get; set; } = new List<MutoonRecord>();

        public async Task OnGetAsync()
        {
            MutoonRecord = await _context.MutoonRecords
                .Include(m => m.Student)
                .OrderByDescending(m => m.RecordDate)
                .ToListAsync();
        }
    }
}