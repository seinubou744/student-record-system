using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages.Timetable
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<TimetableEntry> Entries { get; set; } = new List<TimetableEntry>();

        public async Task OnGetAsync()
        {
            Entries = await _context.TimetableEntries
                .Include(t => t.ClassRoom)
                .Include(t => t.Section)
                .Include(t => t.Subject)
                .Include(t => t.Teacher)
                .OrderBy(t => t.ClassRoom!.Name)
                .ThenBy(t => t.Section!.Name)
                .ThenBy(t => t.DayOfWeek)
                .ThenBy(t => t.StartTime)
                .ToListAsync();
        }
    }
}