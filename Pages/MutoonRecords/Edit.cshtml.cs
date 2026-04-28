using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages_MutoonRecords
{
    public class EditModel : PageModel
    {
        private readonly StudentRecordSystem.Data.ApplicationDbContext _context;

        public EditModel(StudentRecordSystem.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public MutoonRecord MutoonRecord { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mutoonrecord =  await _context.MutoonRecords.FirstOrDefaultAsync(m => m.Id == id);
            if (mutoonrecord == null)
            {
                return NotFound();
            }
            MutoonRecord = mutoonrecord;
           ViewData["StudentId"] = new SelectList(_context.Students, "Id", "AdmissionNo");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(MutoonRecord).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MutoonRecordExists(MutoonRecord.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool MutoonRecordExists(int id)
        {
            return _context.MutoonRecords.Any(e => e.Id == id);
        }
    }
}
