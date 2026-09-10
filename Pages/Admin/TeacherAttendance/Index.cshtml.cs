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
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<StudentRecordSystem.Models.TeacherAttendance> Records { get; set; }
            = new List<StudentRecordSystem.Models.TeacherAttendance>();

        [BindProperty(SupportsGet = true)]
        public int? ClassRoomId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? AttendanceDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public List<SelectListItem> ClassRooms { get; set; } = new();

        public async Task OnGetAsync()
        {
            ClassRooms = await _context.ClassRooms
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();

            var query = _context.TeacherAttendances
                .AsNoTracking()
                .Include(t => t.Student)
                .Include(t => t.ClassRoom)
                .Include(t => t.TeacherUser)
                .AsQueryable();

            if (ClassRoomId.HasValue)
            {
                query = query.Where(t => t.ClassRoomId == ClassRoomId.Value);
            }

            if (AttendanceDate.HasValue)
            {
                var selectedDate = AttendanceDate.Value.Date;
                query = query.Where(t => t.AttendanceDate.Date == selectedDate);
            }

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(t =>
                    t.Student != null &&
                    t.Student.FullName != null &&
                    t.Student.FullName.Contains(SearchTerm));
            }

            Records = await query
                .OrderByDescending(t => t.AttendanceDate)
                .ThenBy(t => t.ClassRoom != null ? t.ClassRoom.Name : "")
                .ThenBy(t => t.Student != null ? t.Student.FullName : "")
                .ToListAsync();
        }
    }
}