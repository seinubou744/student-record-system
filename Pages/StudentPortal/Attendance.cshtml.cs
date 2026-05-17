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

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return;

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id);

            if (student == null)
                return;

            AttendanceRecords = await _context.TeacherAttendances
                .Where(a => a.StudentId == student.Id)
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();
        }
    }
}