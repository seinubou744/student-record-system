using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages.Admin.TeacherAttendance
{
	[Authorize(Roles = "Admin")]
	public class EditModel : PageModel
	{
		private readonly ApplicationDbContext _context;

		public EditModel(ApplicationDbContext context)
		{
			_context = context;
		}

		[BindProperty]
		public StudentRecordSystem.Models.TeacherAttendance TeacherAttendanceRecord { get; set; } = default!;

		public SelectList StudentList { get; set; } = default!;
		public SelectList ClassList { get; set; } = default!;

		public async Task<IActionResult> OnGetAsync(int? id)
		{
			if (id == null) return NotFound();

			var record = await _context.TeacherAttendances.FindAsync(id);
			if (record == null) return NotFound();

			TeacherAttendanceRecord = record;
			await LoadListsAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!ModelState.IsValid)
			{
				await LoadListsAsync();
				return Page();
			}

			_context.Attach(TeacherAttendanceRecord).State = EntityState.Modified;
			await _context.SaveChangesAsync();

			return RedirectToPage("./Index");
		}

		private async Task LoadListsAsync()
		{
			StudentList = new SelectList(await _context.Students.OrderBy(s => s.FullName).ToListAsync(), "Id", "FullName");
			ClassList = new SelectList(await _context.ClassRooms.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
		}
	}
}