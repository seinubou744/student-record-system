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
    public class DetailsModel : PageModel
    {
        private readonly StudentRecordSystem.Data.ApplicationDbContext _context;

        public DetailsModel(StudentRecordSystem.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public Attendance Attendance { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendance = await _context.Attendances.FirstOrDefaultAsync(m => m.Id == id);

            if (attendance is not null)
            {
                Attendance = attendance;

                return Page();
            }

            return NotFound();
        }
    }
}
