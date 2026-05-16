using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages.Students
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Student Student { get; set; } = default!;

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.ClassRoom)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            Student = student;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            try
            {
                var appUserId = student.ApplicationUserId;

                var attendances = await _context.Attendances
                    .Where(a => a.StudentId == student.Id)
                    .ToListAsync();

                var quranRecords = await _context.QuranRecords
                    .Where(q => q.StudentId == student.Id)
                    .ToListAsync();

                var mutoonRecords = await _context.MutoonRecords
                    .Where(m => m.StudentId == student.Id)
                    .ToListAsync();

                var peerRequests = await _context.PeerRequests
                    .Where(p => p.SenderStudentId == student.Id || p.ReceiverStudentId == student.Id)
                    .ToListAsync();

                var studentAttendances = await _context.StudentAttendances
                    .Where(a => a.StudentId == student.Id)
                    .ToListAsync();

                var studentLearningRecords = await _context.StudentLearningRecords
                    .Where(r => r.StudentId == student.Id)
                    .ToListAsync();

                var studentTestRecords = await _context.StudentTestRecords
                    .Where(t => t.StudentId == student.Id)
                    .ToListAsync();

                var teacherAttendances = await _context.TeacherAttendances
                    .Where(t => t.StudentId == student.Id)
                    .ToListAsync();

                if (attendances.Any())
                    _context.Attendances.RemoveRange(attendances);

                if (quranRecords.Any())
                    _context.QuranRecords.RemoveRange(quranRecords);

                if (mutoonRecords.Any())
                    _context.MutoonRecords.RemoveRange(mutoonRecords);

                if (peerRequests.Any())
                    _context.PeerRequests.RemoveRange(peerRequests);

                if (studentAttendances.Any())
                    _context.StudentAttendances.RemoveRange(studentAttendances);

                if (studentLearningRecords.Any())
                    _context.StudentLearningRecords.RemoveRange(studentLearningRecords);

                if (studentTestRecords.Any())
                    _context.StudentTestRecords.RemoveRange(studentTestRecords);

                if (teacherAttendances.Any())
                    _context.TeacherAttendances.RemoveRange(teacherAttendances);

                _context.Students.Remove(student);

                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(appUserId))
                {
                    var user = await _userManager.FindByIdAsync(appUserId);
                    if (user != null)
                    {
                        var result = await _userManager.DeleteAsync(user);

                        if (!result.Succeeded)
                        {
                            ErrorMessage = string.Join("; ", result.Errors.Select(e => e.Description));
                        }
                    }
                }

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.InnerException?.Message ?? ex.Message;

                var reloadStudent = await _context.Students
                    .Include(s => s.ClassRoom)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (reloadStudent != null)
                {
                    Student = reloadStudent;
                }

                return Page();
            }
        }
    }
}