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

        [BindProperty]
        public List<AttendanceInputModel> Items { get; set; } = new();

        public List<SelectListItem> TeacherClasses { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        public string? DebugMessage { get; set; }

        public class AttendanceInputModel
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

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            await LoadTeacherClassesAsync(user.Id);

            if (ClassRoomId == null)
            {
                ModelState.AddModelError(string.Empty, "Please select a class.");
                DebugMessage = "POST failed: ClassRoomId is null.";
                await LoadPageAsync();
                return Page();
            }

            var allowed = await _context.TeacherClasses
                .AnyAsync(tc => tc.TeacherUserId == user.Id && tc.ClassRoomId == ClassRoomId.Value);

            if (!allowed)
            {
                DebugMessage = $"POST failed: Teacher {user.Id} is not assigned to class {ClassRoomId.Value}.";
                return Forbid();
            }

            if (!Items.Any())
            {
                ModelState.AddModelError(string.Empty, "No students were loaded for this class.");
                DebugMessage = $"POST failed: No Items were submitted for class {ClassRoomId.Value}.";
                await LoadPageAsync();
                return Page();
            }

            var selectedDate = DateTime.SpecifyKind(AttendanceDate.Date, DateTimeKind.Utc);

            var existing = await _context.TeacherAttendances
                .Where(a => a.ClassRoomId == ClassRoomId.Value && a.AttendanceDate == selectedDate)
                .ToListAsync();

            if (existing.Any())
            {
                _context.TeacherAttendances.RemoveRange(existing);
            }

            foreach (var item in Items)
            {
                _context.TeacherAttendances.Add(new TeacherAttendance
                {
                    StudentId = item.StudentId,
                    ClassRoomId = item.ClassRoomId,
                    AttendanceDate = selectedDate,
                    Status = item.Status,
                    Remarks = item.Remarks,
                    TeacherUserId = user.Id
                });
            }

            await _context.SaveChangesAsync();
            SuccessMessage = "Attendance saved successfully.";

            return RedirectToPage(new
            {
                ClassRoomId,
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
                Items = new List<AttendanceInputModel>();
                return;
            }

            var selectedDate = DateTime.SpecifyKind(AttendanceDate.Date, DateTimeKind.Utc);

            Items = await _context.Students
                .Where(s => s.ClassRoomId == ClassRoomId.Value)
                .OrderBy(s => s.FullName)
                .Select(s => new AttendanceInputModel
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
                .Where(a => a.ClassRoomId == ClassRoomId.Value && a.AttendanceDate == selectedDate)
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