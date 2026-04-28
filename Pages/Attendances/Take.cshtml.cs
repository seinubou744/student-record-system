using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages.Attendances
{
	[Authorize(Roles = "Admin")]
	public class TakeModel : PageModel
	{
		private readonly ApplicationDbContext _context;

		public TakeModel(ApplicationDbContext context)
		{
			_context = context;
		}

		[BindProperty(SupportsGet = true)]
		public int? SelectedSubjectId { get; set; }

		[BindProperty(SupportsGet = true)]
		public DateTime AttendanceDate { get; set; } = DateTime.Today;

		[BindProperty]
		public List<StudentAttendanceInput> StudentsToTake { get; set; } = new();

		public SelectList SubjectList { get; set; } = default!;

		public async Task OnGetAsync()
		{
			await LoadSubjectsAsync();

			if (!SelectedSubjectId.HasValue)
			{
				return;
			}

			var subject = await _context.Subjects
				.FirstOrDefaultAsync(s => s.Id == SelectedSubjectId.Value);

			if (subject == null)
			{
				return;
			}

			StudentsToTake = await _context.Students
				.Where(s => s.ClassRoomId == subject.ClassRoomId)
				.OrderBy(s => s.FullName)
				.Select(s => new StudentAttendanceInput
				{
					StudentId = s.Id,
					AdmissionNo = s.AdmissionNo,
					FullName = s.FullName,
					IsPresent = true
				})
				.ToListAsync();

			var existingAttendance = await _context.Attendances
				.Where(a => a.SubjectId == SelectedSubjectId.Value && a.AttendanceDate == AttendanceDate)
				.ToListAsync();

			foreach (var student in StudentsToTake)
			{
				var existing = existingAttendance.FirstOrDefault(a => a.StudentId == student.StudentId);
				if (existing != null)
				{
					student.IsPresent = existing.IsPresent;
				}
			}
		}

		public async Task<IActionResult> OnPostAsync()
		{
			await LoadSubjectsAsync();

			if (!SelectedSubjectId.HasValue)
			{
				ModelState.AddModelError(string.Empty, "Please select a subject.");
				return Page();
			}

			foreach (var item in StudentsToTake)
			{
				var existing = await _context.Attendances.FirstOrDefaultAsync(a =>
					a.StudentId == item.StudentId &&
					a.SubjectId == SelectedSubjectId.Value &&
					a.AttendanceDate == AttendanceDate);

				if (existing == null)
				{
					_context.Attendances.Add(new Attendance
					{
						StudentId = item.StudentId,
						SubjectId = SelectedSubjectId.Value,
						AttendanceDate = AttendanceDate,
						IsPresent = item.IsPresent
					});
				}
				else
				{
					existing.IsPresent = item.IsPresent;
				}
			}

			await _context.SaveChangesAsync();

			TempData["SuccessMessage"] = "Attendance saved successfully.";
			return RedirectToPage(new { SelectedSubjectId, AttendanceDate });
		}

		private async Task LoadSubjectsAsync()
		{
			var subjects = await _context.Subjects
				.Include(s => s.ClassRoom)
				.OrderBy(s => s.Name)
				.Select(s => new
				{
					s.Id,
					DisplayName = s.Name + " - " + (s.ClassRoom != null ? s.ClassRoom.Name : "")
				})
				.ToListAsync();

			SubjectList = new SelectList(subjects, "Id", "DisplayName");
		}

		public class StudentAttendanceInput
		{
			public int StudentId { get; set; }
			public string AdmissionNo { get; set; } = string.Empty;
			public string FullName { get; set; } = string.Empty;
			public bool IsPresent { get; set; }
		}
	}
}