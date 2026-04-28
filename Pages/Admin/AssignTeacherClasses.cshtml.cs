using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class AssignTeacherClassesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AssignTeacherClassesModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public string? TeacherUserId { get; set; }

        [BindProperty]
        public List<int> SelectedClassRoomIds { get; set; } = new();

        public List<SelectListItem> Teachers { get; set; } = new();
        public List<ClassRoomSelectionViewModel> AvailableClasses { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        public class ClassRoomSelectionViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public bool Assigned { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadTeachersAsync();

            if (!string.IsNullOrEmpty(TeacherUserId))
            {
                await LoadClassesAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadTeachersAsync();

            if (string.IsNullOrEmpty(TeacherUserId))
            {
                ModelState.AddModelError(string.Empty, "Please select a teacher.");
                await LoadClassesAsync();
                return Page();
            }

            var teacher = await _userManager.FindByIdAsync(TeacherUserId);
            if (teacher == null)
            {
                ModelState.AddModelError(string.Empty, "Selected teacher was not found.");
                await LoadClassesAsync();
                return Page();
            }

            var isTeacher = await _userManager.IsInRoleAsync(teacher, "Teacher");
            if (!isTeacher)
            {
                ModelState.AddModelError(string.Empty, "Selected user is not in the Teacher role.");
                await LoadClassesAsync();
                return Page();
            }

            var existingAssignments = await _context.TeacherClasses
                .Where(tc => tc.TeacherUserId == TeacherUserId)
                .ToListAsync();

            if (existingAssignments.Any())
            {
                _context.TeacherClasses.RemoveRange(existingAssignments);
            }

            foreach (var classRoomId in SelectedClassRoomIds.Distinct())
            {
                _context.TeacherClasses.Add(new TeacherClass
                {
                    TeacherUserId = TeacherUserId,
                    ClassRoomId = classRoomId
                });
            }

            await _context.SaveChangesAsync();

            SuccessMessage = "Teacher class assignments updated successfully.";
            return RedirectToPage(new { TeacherUserId });
        }

        private async Task LoadTeachersAsync()
        {
            var usersInTeacherRole = await _userManager.GetUsersInRoleAsync("Teacher");

            Teachers = usersInTeacherRole
                .OrderBy(t => t.FullName)
                .Select(t => new SelectListItem
                {
                    Value = t.Id,
                    Text = $"{t.FullName} ({t.Email})"
                })
                .ToList();
        }

        private async Task LoadClassesAsync()
        {
            var assignedClassIds = new List<int>();

            if (!string.IsNullOrEmpty(TeacherUserId))
            {
                assignedClassIds = await _context.TeacherClasses
                    .Where(tc => tc.TeacherUserId == TeacherUserId)
                    .Select(tc => tc.ClassRoomId)
                    .ToListAsync();
            }

            AvailableClasses = await _context.ClassRooms
                .OrderBy(c => c.Name)
                .Select(c => new ClassRoomSelectionViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Assigned = assignedClassIds.Contains(c.Id)
                })
                .ToListAsync();

            if (!SelectedClassRoomIds.Any())
            {
                SelectedClassRoomIds = assignedClassIds;
            }
        }
    }
}