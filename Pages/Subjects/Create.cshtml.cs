using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages_Subjects
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Subject Subject { get; set; } = new();

        public SelectList ClassRoomList { get; set; } = default!;

        public async Task OnGetAsync()
        {
            await LoadClassRoomsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadClassRoomsAsync();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var subjectExists = await _context.Subjects
                .AnyAsync(s => s.Name == Subject.Name && s.ClassRoomId == Subject.ClassRoomId);

            if (subjectExists)
            {
                ModelState.AddModelError(string.Empty, "This subject already exists for the selected class room.");
                return Page();
            }

            _context.Subjects.Add(Subject);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Subject created successfully.";
            return RedirectToPage("./Index");
        }

        private async Task LoadClassRoomsAsync()
        {
            var classRooms = await _context.ClassRooms
                .OrderBy(c => c.Name)
                .ToListAsync();

            ClassRoomList = new SelectList(classRooms, "Id", "Name");
        }
    }
}