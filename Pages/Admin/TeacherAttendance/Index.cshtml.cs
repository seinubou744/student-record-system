using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages.Admin.TeacherAttendance
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<StudentRecordSystem.Models.TeacherAttendance> Records { get; set; } = new List<StudentRecordSystem.Models.TeacherAttendance>();

        public async Task OnGetAsync()
        {
            Records = await _context.TeacherAttendances
                .Include(t => t.Student)
                .Include(t => t.ClassRoom)
                .Include(t => t.TeacherUser)
                .OrderByDescending(t => t.AttendanceDate)
                .ThenBy(t => t.Student!.FullName)
                .ToListAsync();
        }
    }
}