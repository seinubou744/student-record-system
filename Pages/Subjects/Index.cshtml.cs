using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages_Subjects
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Subject> Subject { get; set; } = new List<Subject>();

        public async Task OnGetAsync()
        {
            Subject = await _context.Subjects
                .Include(s => s.ClassRoom)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
    }
}