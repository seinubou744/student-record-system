using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages.Attendances
{
    public class IndexModel : PageModel
    {
        private readonly StudentRecordSystem.Data.ApplicationDbContext _context;

        public IndexModel(StudentRecordSystem.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Attendance> Attendance { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Attendance = await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Subject).ToListAsync();
        }
    }
}
