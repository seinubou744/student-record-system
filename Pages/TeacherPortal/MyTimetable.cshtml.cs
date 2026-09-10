using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages.TeacherPortal
{
	[Authorize(Roles = "Teacher")]
	public class MyTimetableModel : PageModel
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public MyTimetableModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		[BindProperty(SupportsGet = true)]
		public WeekDay? Day { get; set; }

		[BindProperty(SupportsGet = true)]
		public string? AcademicYear { get; set; }

		public IList<TimetableEntry> Entries { get; set; } = new List<TimetableEntry>();

		public async Task<IActionResult> OnGetAsync()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user is null) return Challenge();

			var query = _context.TimetableEntries
				.Include(t => t.ClassRoom)
				.Include(t => t.Section)
				.Include(t => t.Subject)
				.Where(t => t.TeacherId == user.Id);

			if (!string.IsNullOrWhiteSpace(AcademicYear))
				query = query.Where(t => t.AcademicYear == AcademicYear);

			if (Day.HasValue)
				query = query.Where(t => t.DayOfWeek == Day.Value);

			Entries = await query
				.OrderBy(t => t.DayOfWeek)
				.ThenBy(t => t.StartTime)
				.ToListAsync();

			return Page();
		}
	}
}