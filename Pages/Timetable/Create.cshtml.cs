using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;
using StudentRecordSystem.Services;

namespace StudentRecordSystem.Pages.Timetable
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TimetableService _timetableService;

        public CreateModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            TimetableService timetableService)
        {
            _context = context;
            _userManager = userManager;
            _timetableService = timetableService;
        }

        [BindProperty]
        public InputModel Entry { get; set; } = new()
        {
            AcademicYear = "2026-2027"
        };

        public List<SelectListItem> ClassRooms { get; set; } = new();
        public List<SelectListItem> Sections { get; set; } = new();
        public List<SelectListItem> Subjects { get; set; } = new();
        public List<SelectListItem> Teachers { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Please select a class.")]
            public int? ClassRoomId { get; set; }

            [Required(ErrorMessage = "Please select a section.")]
            public int? SectionId { get; set; }

            [Required(ErrorMessage = "Please select a subject.")]
            public int? SubjectId { get; set; }

            [Required(ErrorMessage = "Please select a teacher.")]
            public string? TeacherId { get; set; }

            [Required(ErrorMessage = "Please select a day.")]
            public WeekDay? DayOfWeek { get; set; }

            [Required(ErrorMessage = "Please enter a start time.")]
            public TimeSpan? StartTime { get; set; }

            [Required(ErrorMessage = "Please enter an end time.")]
            public TimeSpan? EndTime { get; set; }

            [StringLength(50)]
            public string? RoomNumber { get; set; }

            [Required(ErrorMessage = "Please enter the academic year.")]
            [StringLength(20)]
            public string AcademicYear { get; set; } = string.Empty;
        }

        public async Task OnGetAsync()
        {
            await LoadListsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadListsAsync();

            if (Entry.StartTime.HasValue && Entry.EndTime.HasValue && Entry.StartTime.Value >= Entry.EndTime.Value)
            {
                ModelState.AddModelError(nameof(Entry.EndTime), "End time must be later than start time.");
            }

            if (Entry.ClassRoomId.HasValue && Entry.SectionId.HasValue)
            {
                var validSection = await _context.Sections
                    .AnyAsync(s => s.Id == Entry.SectionId.Value && s.ClassRoomId == Entry.ClassRoomId.Value);

                if (!validSection)
                {
                    ModelState.AddModelError(nameof(Entry.SectionId), "Selected section does not belong to the selected class.");
                }
            }

            if (Entry.ClassRoomId.HasValue && Entry.SubjectId.HasValue)
            {
                var validSubject = await _context.Subjects
                    .AnyAsync(s => s.Id == Entry.SubjectId.Value && s.ClassRoomId == Entry.ClassRoomId.Value);

                if (!validSubject)
                {
                    ModelState.AddModelError(nameof(Entry.SubjectId), "Selected subject does not belong to the selected class.");
                }
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var timetableEntry = new TimetableEntry
            {
                ClassRoomId = Entry.ClassRoomId!.Value,
                SectionId = Entry.SectionId!.Value,
                SubjectId = Entry.SubjectId!.Value,
                TeacherId = Entry.TeacherId!,
                DayOfWeek = Entry.DayOfWeek!.Value,
                StartTime = Entry.StartTime!.Value,
                EndTime = Entry.EndTime!.Value,
                RoomNumber = string.IsNullOrWhiteSpace(Entry.RoomNumber) ? null : Entry.RoomNumber.Trim(),
                AcademicYear = Entry.AcademicYear.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var validationMessage = await _timetableService.ValidateEntryAsync(timetableEntry);
            if (!string.IsNullOrWhiteSpace(validationMessage))
            {
                ModelState.AddModelError(string.Empty, validationMessage);
                return Page();
            }

            _context.TimetableEntries.Add(timetableEntry);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Schedule saved successfully.";
            return RedirectToPage("Index");
        }

        private async Task LoadListsAsync()
        {
            ClassRooms = await _context.ClassRooms
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();

            var selectedClassRoomId = Entry.ClassRoomId;

            Sections = await _context.Sections
                .Where(s => !selectedClassRoomId.HasValue || s.ClassRoomId == selectedClassRoomId.Value)
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync();

            Subjects = await _context.Subjects
                .Where(s => !selectedClassRoomId.HasValue || s.ClassRoomId == selectedClassRoomId.Value)
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync();

            var teachersInRole = await _userManager.GetUsersInRoleAsync("Teacher");
            Teachers = teachersInRole
                .OrderBy(t => t.FullName)
                .Select(t => new SelectListItem
                {
                    Value = t.Id,
                    Text = t.FullName
                })
                .ToList();
        }
    }
}