using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;

namespace StudentRecordSystem.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalStudents { get; set; }
        public int TotalClassRooms { get; set; }
        public int TotalSubjects { get; set; }
        public int TotalAttendances { get; set; }

        public async Task OnGetAsync()
        {
            if (User.IsInRole("Admin"))
            {
                TotalStudents = await _context.Students.CountAsync();
                TotalClassRooms = await _context.ClassRooms.CountAsync();
                TotalSubjects = await _context.Subjects.CountAsync();
                TotalAttendances = await _context.Attendances.CountAsync();
            }
        }
    }
}