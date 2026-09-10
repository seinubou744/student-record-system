using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages.StudentPortal
{
    [Authorize(Roles = "Student")]
    public class AttendanceModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AttendanceModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<TeacherAttendance> AttendanceRecords { get; set; } = new List<TeacherAttendance>();

        public Student? CurrentStudent { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return;

            CurrentStudent = await _context.Students
                .AsNoTracking()
                .Include(s => s.ClassRoom)
                .FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id);

            if (CurrentStudent == null)
                return;

            AttendanceRecords = await _context.TeacherAttendances
                .AsNoTracking()
                .Where(a => a.StudentId == CurrentStudent.Id)
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();
        }
    }
}