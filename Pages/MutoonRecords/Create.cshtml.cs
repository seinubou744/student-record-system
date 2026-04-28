using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages_MutoonRecords
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public MutoonRecord MutoonRecord { get; set; } = default!;

        public SelectList StudentList { get; set; } = default!;
        public SelectList MatnList { get; set; } = default!;

        public IActionResult OnGet()
        {
            LoadDropDowns();
            MutoonRecord = new MutoonRecord
            {
                RecordDate = DateTime.Today
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                LoadDropDowns();
                return Page();
            }

            _context.MutoonRecords.Add(MutoonRecord);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        private void LoadDropDowns()
        {
            StudentList = new SelectList(
                _context.Students
                    .OrderBy(s => s.FullName)
                    .Select(s => new
                    {
                        s.Id,
                        Name = s.FullName + " (" + s.AdmissionNo + ")"
                    })
                    .ToList(),
                "Id",
                "Name"
            );

            var mutoon = new List<string>
            {
                "Al-Usul Ath-Thalatha",
                "Al-Qawaid Al-Arba",
                "Kashf Ash-Shubuhat",
                "Kitab At-Tawhid",
                "Nawaqid Al-Islam",
                "Al-Aqidah Al-Wasitiyyah",
                "Al-Aqidah At-Tahawiyyah",
                "Al-Bayquniyyah",
                "Al-Ajrumiyyah",
                "Umdat Al-Ahkam",
                "Bulugh Al-Maram",
                "Arba'in An-Nawawiyyah",
                "Riyad As-Salihin",
                "Tuhfat Al-Atfal",
                "Al-Jazariyyah"
            };

            MatnList = new SelectList(mutoon);
        }
    }
}