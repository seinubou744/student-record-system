using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages.StudentPortal
{
    [Authorize(Roles = "Student")]
    public class MyScheduleModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyScheduleModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<TimetableEntry> Entries { get; set; } = new List<TimetableEntry>();
        public Student? CurrentStudent { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return Challenge();

            CurrentStudent = await _context.Students
                .Include(s => s.ClassRoom)
                .Include(s => s.Section)
                .FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id);

            if (CurrentStudent is null)
            {
                ErrorMessage = "Student profile was not found.";
                return Page();
            }

            if (CurrentStudent.SectionId <= 0)
            {
                ErrorMessage = "Your student account is not assigned to a section yet. Please contact the administrator.";
                return Page();
            }

            Entries = await _context.TimetableEntries
                .Include(t => t.Subject)
                .Include(t => t.Teacher)
                .Where(t =>
                    t.ClassRoomId == CurrentStudent.ClassRoomId &&
                    t.SectionId == CurrentStudent.SectionId)
                .OrderBy(t => t.DayOfWeek)
                .ThenBy(t => t.StartTime)
                .ToListAsync();

            return Page();
        }
    }
}