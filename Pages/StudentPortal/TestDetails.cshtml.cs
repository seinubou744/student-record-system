using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages.StudentPortal
{
    [Authorize(Roles = "Student")]
    public class TestDetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TestDetailsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<StudentTestRecord> TestRecords { get; set; } = new();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id);

            if (student == null) return;

            TestRecords = await _context.StudentTestRecords
                .Where(t => t.StudentId == student.Id)
                .OrderByDescending(t => t.TestDate)
                .ToListAsync();
        }
    }
}