using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages.StudentPortal
{
    [Authorize(Roles = "Student")]
    public class MutoonProgressModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MutoonProgressModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<StudentLearningRecord> MutoonRecords { get; set; } = new List<StudentLearningRecord>();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return;

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id);

            if (student == null)
                return;

            MutoonRecords = await _context.StudentLearningRecords
                .Where(m => m.StudentId == student.Id &&
                            m.RecordType == LearningRecordType.Mutoon)
                .OrderByDescending(m => m.RecordDate)
                .ToListAsync();
        }
    }
}