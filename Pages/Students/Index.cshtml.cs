using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages_Students
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Student> Student { get; set; } = new List<Student>();

        public async Task OnGetAsync()
        {
            Student = await _context.Students
                .Include(s => s.ClassRoom)
                .Include(s => s.Section)
                .OrderBy(s => s.FullName)
                .ToListAsync();
        }
    }
}