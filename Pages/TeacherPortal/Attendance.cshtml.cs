using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages.TeacherPortal
{
    [Authorize(Roles = "Teacher")]
    public class AttendanceModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AttendanceModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public int? ClassRoomId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime AttendanceDate { get; set; } = DateTime.UtcNow.Date;

        public List<AttendanceRowViewModel> Items { get; set; } = new();

        public List<SelectListItem> TeacherClasses { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public string? DebugMessage { get; set; }

        public class AttendanceRowViewModel
        {
            public int StudentId { get; set; }
            public string AdmissionNo { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public int ClassRoomId { get; set; }
            public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
            public string? Remarks { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadPageAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostSaveStudentAsync(
            int studentId,
            int classRoomId,
            DateTime attendanceDate,
            AttendanceStatus status,
            string? remarks)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            ClassRoomId = classRoomId;
            AttendanceDate = attendanceDate.Date;

            await LoadTeacherClassesAsync(user.Id);

            var allowed = await _context.TeacherClasses
                .AnyAsync(tc => tc.TeacherUserId == user.Id && tc.ClassRoomId == classRoomId);

            if (!allowed)
            {
                return Forbid();
            }

            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == studentId && s.ClassRoomId == classRoomId);

            if (student == null)
            {
                ErrorMessage = "Selected student was not found in this class.";
                return RedirectToPage(new
                {
                    ClassRoomId = classRoomId,
                    AttendanceDate = attendanceDate.ToString("yyyy-MM-dd")
                });
            }

            var selectedDate = attendanceDate.Date;

            var existing = await _context.TeacherAttendances
                .FirstOrDefaultAsync(a =>
                    a.StudentId == studentId &&
                    a.ClassRoomId == classRoomId &&
                    a.AttendanceDate.Date == selectedDate);

            if (existing == null)
            {
                _context.TeacherAttendances.Add(new TeacherAttendance
                {
                    StudentId = studentId,
                    ClassRoomId = classRoomId,
                    AttendanceDate = selectedDate,
                    Status = status,
                    Remarks = string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim(),
                    TeacherUserId = user.Id
                });
            }
            else
            {
                existing.Status = status;
                existing.Remarks = string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim();
                existing.TeacherUserId = user.Id;
            }

            await _context.SaveChangesAsync();

            SuccessMessage = $"Attendance saved for {student.FullName}.";
            return RedirectToPage(new
            {
                ClassRoomId = classRoomId,
                AttendanceDate = selectedDate.ToString("yyyy-MM-dd")
            });
        }

        private async Task LoadPageAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                DebugMessage = "No logged in teacher found.";
                return;
            }

            await LoadTeacherClassesAsync(user.Id);

            if (!TeacherClasses.Any())
            {
                DebugMessage = $"No classes assigned to teacher {user.Id}.";
                return;
            }

            if (ClassRoomId == null)
            {
                ClassRoomId = int.Parse(TeacherClasses.First().Value);
            }

            var isAssigned = TeacherClasses.Any(c => c.Value == ClassRoomId.Value.ToString());
            if (!isAssigned)
            {
                DebugMessage = $"Selected class {ClassRoomId.Value} is not in the teacher assigned classes list.";
                Items = new List<AttendanceRowViewModel>();
                return;
            }

            var selectedDate = AttendanceDate.Date;

            Items = await _context.Students
                .AsNoTracking()
                .Where(s => s.ClassRoomId == ClassRoomId.Value)
                .OrderBy(s => s.FullName)
                .Select(s => new AttendanceRowViewModel
                {
                    StudentId = s.Id,
                    AdmissionNo = s.AdmissionNo,
                    FullName = s.FullName,
                    ClassRoomId = s.ClassRoomId
                })
                .ToListAsync();

            if (!Items.Any())
            {
                DebugMessage = $"No students found in class {ClassRoomId.Value}.";
                return;
            }

            var existing = await _context.TeacherAttendances
                .AsNoTracking()
                .Where(a => a.ClassRoomId == ClassRoomId.Value && a.AttendanceDate.Date == selectedDate)
                .ToListAsync();

            foreach (var item in Items)
            {
                var saved = existing.FirstOrDefault(x => x.StudentId == item.StudentId);
                if (saved != null)
                {
                    item.Status = saved.Status;
                    item.Remarks = saved.Remarks;
                }
            }

            DebugMessage = $"Loaded {Items.Count} students for class {ClassRoomId.Value}.";
        }

        private async Task LoadTeacherClassesAsync(string teacherUserId)
        {
            TeacherClasses = await _context.TeacherClasses
                .Where(tc => tc.TeacherUserId == teacherUserId && tc.ClassRoom != null)
                .Include(tc => tc.ClassRoom)
                .OrderBy(tc => tc.ClassRoom!.Name)
                .Select(tc => new SelectListItem
                {
                    Value = tc.ClassRoomId.ToString(),
                    Text = tc.ClassRoom!.Name
                })
                .ToListAsync();
        }
    }
}