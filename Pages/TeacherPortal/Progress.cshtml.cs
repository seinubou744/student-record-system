using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages.TeacherPortal
{
    [Authorize(Roles = "Teacher")]
    public class ProgressModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProgressModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public int? ClassRoomId { get; set; }

        [BindProperty(SupportsGet = true)]
        public LearningRecordType RecordType { get; set; } = LearningRecordType.Quran;

        [BindProperty(SupportsGet = true)]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow.Date;

        [BindProperty]
        public List<ProgressInputModel> Items { get; set; } = new();

        public List<SelectListItem> TeacherClasses { get; set; } = new();
        public List<SelectListItem> SurahOptions { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        public class ProgressInputModel
        {
            public int StudentId { get; set; }
            public string StudentCode { get; set; } = string.Empty;
            public string StudentName { get; set; } = string.Empty;
            public int ClassRoomId { get; set; }

            public string? SurahName { get; set; }
            public int? FromAyah { get; set; }
            public int? ToAyah { get; set; }

            public string? MatnName { get; set; }
            public string? FromPortion { get; set; }
            public string? ToPortion { get; set; }

            public string? Remarks { get; set; }
        }

        public async Task OnGetAsync()
        {
            LoadSurahOptions();
            await LoadPageAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            LoadSurahOptions();
            await LoadTeacherClassesAsync(user.Id);

            if (ClassRoomId == null)
            {
                ModelState.AddModelError(string.Empty, "Please select a class.");
                await LoadPageAsync();
                return Page();
            }

            var allowed = await _context.TeacherClasses
                .AnyAsync(tc => tc.TeacherUserId == user.Id && tc.ClassRoomId == ClassRoomId.Value);

            if (!allowed)
            {
                return Forbid();
            }

            var selectedDate = DateTime.SpecifyKind(RecordDate.Date, DateTimeKind.Utc);

            if (RecordType == LearningRecordType.Quran)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    var item = Items[i];

                    var hasAnyQuranField =
                        !string.IsNullOrWhiteSpace(item.SurahName) ||
                        item.FromAyah.HasValue ||
                        item.ToAyah.HasValue;

                    if (hasAnyQuranField)
                    {
                        if (string.IsNullOrWhiteSpace(item.SurahName))
                        {
                            ModelState.AddModelError(string.Empty, $"Please select a surah for {item.StudentName}.");
                        }

                        if (!item.FromAyah.HasValue)
                        {
                            ModelState.AddModelError(string.Empty, $"Please enter From Ayah for {item.StudentName}.");
                        }

                        if (!item.ToAyah.HasValue)
                        {
                            ModelState.AddModelError(string.Empty, $"Please enter To Ayah for {item.StudentName}.");
                        }

                        if (item.FromAyah.HasValue && item.ToAyah.HasValue && item.FromAyah > item.ToAyah)
                        {
                            ModelState.AddModelError(string.Empty, $"From Ayah cannot be greater than To Ayah for {item.StudentName}.");
                        }
                    }
                }
            }
            else if (RecordType == LearningRecordType.Mutoon)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    var item = Items[i];

                    var hasAnyMutoonField =
                        !string.IsNullOrWhiteSpace(item.MatnName) ||
                        !string.IsNullOrWhiteSpace(item.FromPortion) ||
                        !string.IsNullOrWhiteSpace(item.ToPortion);

                    if (hasAnyMutoonField)
                    {
                        if (string.IsNullOrWhiteSpace(item.MatnName))
                        {
                            ModelState.AddModelError(string.Empty, $"Please enter Matn Name for {item.StudentName}.");
                        }

                        if (string.IsNullOrWhiteSpace(item.FromPortion))
                        {
                            ModelState.AddModelError(string.Empty, $"Please enter From Portion for {item.StudentName}.");
                        }

                        if (string.IsNullOrWhiteSpace(item.ToPortion))
                        {
                            ModelState.AddModelError(string.Empty, $"Please enter To Portion for {item.StudentName}.");
                        }
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadPageAsync();
                return Page();
            }

            var existing = await _context.StudentLearningRecords
                .Where(r => r.ClassRoomId == ClassRoomId.Value &&
                            r.RecordDate == selectedDate &&
                            r.RecordType == RecordType)
                .ToListAsync();

            if (existing.Any())
            {
                _context.StudentLearningRecords.RemoveRange(existing);
            }

            foreach (var item in Items)
            {
                _context.StudentLearningRecords.Add(new StudentLearningRecord
                {
                    StudentId = item.StudentId,
                    ClassRoomId = item.ClassRoomId,
                    RecordType = RecordType,
                    RecordDate = selectedDate,
                    SurahName = RecordType == LearningRecordType.Quran ? item.SurahName : null,
                    FromAyah = RecordType == LearningRecordType.Quran ? item.FromAyah : null,
                    ToAyah = RecordType == LearningRecordType.Quran ? item.ToAyah : null,
                    MatnName = RecordType == LearningRecordType.Mutoon ? item.MatnName : null,
                    FromPortion = RecordType == LearningRecordType.Mutoon ? item.FromPortion : null,
                    ToPortion = RecordType == LearningRecordType.Mutoon ? item.ToPortion : null,
                    Remarks = item.Remarks,
                    TeacherUserId = user.Id
                });
            }

            await _context.SaveChangesAsync();

            SuccessMessage = $"{RecordType} records saved successfully.";
            return RedirectToPage(new
            {
                ClassRoomId,
                RecordType,
                RecordDate = selectedDate.ToString("yyyy-MM-dd")
            });
        }

        private async Task LoadPageAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            LoadSurahOptions();
            await LoadTeacherClassesAsync(user.Id);

            if (ClassRoomId == null && TeacherClasses.Any())
            {
                ClassRoomId = int.Parse(TeacherClasses.First().Value);
            }

            if (ClassRoomId == null) return;

            var selectedDate = DateTime.SpecifyKind(RecordDate.Date, DateTimeKind.Utc);

            Items = await _context.Students
                .Where(s => s.ClassRoomId == ClassRoomId.Value)
                .OrderBy(s => s.FullName)
                .Select(s => new ProgressInputModel
                {
                    StudentId = s.Id,
                    StudentCode = s.AdmissionNo,
                    StudentName = s.FullName,
                    ClassRoomId = s.ClassRoomId
                })
                .ToListAsync();

            var existing = await _context.StudentLearningRecords
                .Where(r => r.ClassRoomId == ClassRoomId.Value &&
                            r.RecordDate == selectedDate &&
                            r.RecordType == RecordType)
                .ToListAsync();

            foreach (var item in Items)
            {
                var saved = existing.FirstOrDefault(x => x.StudentId == item.StudentId);
                if (saved != null)
                {
                    item.SurahName = saved.SurahName;
                    item.FromAyah = saved.FromAyah;
                    item.ToAyah = saved.ToAyah;
                    item.MatnName = saved.MatnName;
                    item.FromPortion = saved.FromPortion;
                    item.ToPortion = saved.ToPortion;
                    item.Remarks = saved.Remarks;
                }
            }
        }

        private async Task LoadTeacherClassesAsync(string teacherUserId)
        {
            TeacherClasses = await _context.TeacherClasses
                .Where(tc => tc.TeacherUserId == teacherUserId && tc.ClassRoom != null)
                .Include(tc => tc.ClassRoom)
                .OrderBy(tc => tc.ClassRoom!.Name)
                .Select(tc => new SelectListItem
                {
                    Value = tc.ClassRoomId.ToString(),
                    Text = tc.ClassRoom!.Name
                })
                .ToListAsync();
        }

        private void LoadSurahOptions()
        {
            SurahOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "Al-Fatihah", Text = "1. Al-Fatihah" },
                new SelectListItem { Value = "Al-Baqarah", Text = "2. Al-Baqarah" },
                new SelectListItem { Value = "Aal-E-Imran", Text = "3. Aal-E-Imran" },
                new SelectListItem { Value = "An-Nisa", Text = "4. An-Nisa" },
                new SelectListItem { Value = "Al-Ma'idah", Text = "5. Al-Ma'idah" },
                new SelectListItem { Value = "Al-An'am", Text = "6. Al-An'am" },
                new SelectListItem { Value = "Al-A'raf", Text = "7. Al-A'raf" },
                new SelectListItem { Value = "Al-Anfal", Text = "8. Al-Anfal" },
                new SelectListItem { Value = "At-Tawbah", Text = "9. At-Tawbah" },
                new SelectListItem { Value = "Yunus", Text = "10. Yunus" },
                new SelectListItem { Value = "Hud", Text = "11. Hud" },
                new SelectListItem { Value = "Yusuf", Text = "12. Yusuf" },
                new SelectListItem { Value = "Ar-Ra'd", Text = "13. Ar-Ra'd" },
                new SelectListItem { Value = "Ibrahim", Text = "14. Ibrahim" },
                new SelectListItem { Value = "Al-Hijr", Text = "15. Al-Hijr" },
                new SelectListItem { Value = "An-Nahl", Text = "16. An-Nahl" },
                new SelectListItem { Value = "Al-Isra", Text = "17. Al-Isra" },
                new SelectListItem { Value = "Al-Kahf", Text = "18. Al-Kahf" },
                new SelectListItem { Value = "Maryam", Text = "19. Maryam" },
                new SelectListItem { Value = "Ta-Ha", Text = "20. Ta-Ha" },
                new SelectListItem { Value = "Al-Anbiya", Text = "21. Al-Anbiya" },
                new SelectListItem { Value = "Al-Hajj", Text = "22. Al-Hajj" },
                new SelectListItem { Value = "Al-Mu'minun", Text = "23. Al-Mu'minun" },
                new SelectListItem { Value = "An-Nur", Text = "24. An-Nur" },
                new SelectListItem { Value = "Al-Furqan", Text = "25. Al-Furqan" },
                new SelectListItem { Value = "Ash-Shu'ara", Text = "26. Ash-Shu'ara" },
                new SelectListItem { Value = "An-Naml", Text = "27. An-Naml" },
                new SelectListItem { Value = "Al-Qasas", Text = "28. Al-Qasas" },
                new SelectListItem { Value = "Al-Ankabut", Text = "29. Al-Ankabut" },
                new SelectListItem { Value = "Ar-Rum", Text = "30. Ar-Rum" },
                new SelectListItem { Value = "Luqman", Text = "31. Luqman" },
                new SelectListItem { Value = "As-Sajdah", Text = "32. As-Sajdah" },
                new SelectListItem { Value = "Al-Ahzab", Text = "33. Al-Ahzab" },
                new SelectListItem { Value = "Saba", Text = "34. Saba" },
                new SelectListItem { Value = "Fatir", Text = "35. Fatir" },
                new SelectListItem { Value = "Ya-Sin", Text = "36. Ya-Sin" },
                new SelectListItem { Value = "As-Saffat", Text = "37. As-Saffat" },
                new SelectListItem { Value = "Sad", Text = "38. Sad" },
                new SelectListItem { Value = "Az-Zumar", Text = "39. Az-Zumar" },
                new SelectListItem { Value = "Ghafir", Text = "40. Ghafir" },
                new SelectListItem { Value = "Fussilat", Text = "41. Fussilat" },
                new SelectListItem { Value = "Ash-Shura", Text = "42. Ash-Shura" },
                new SelectListItem { Value = "Az-Zukhruf", Text = "43. Az-Zukhruf" },
                new SelectListItem { Value = "Ad-Dukhan", Text = "44. Ad-Dukhan" },
                new SelectListItem { Value = "Al-Jathiyah", Text = "45. Al-Jathiyah" },
                new SelectListItem { Value = "Al-Ahqaf", Text = "46. Al-Ahqaf" },
                new SelectListItem { Value = "Muhammad", Text = "47. Muhammad" },
                new SelectListItem { Value = "Al-Fath", Text = "48. Al-Fath" },
                new SelectListItem { Value = "Al-Hujurat", Text = "49. Al-Hujurat" },
                new SelectListItem { Value = "Qaf", Text = "50. Qaf" },
                new SelectListItem { Value = "Adh-Dhariyat", Text = "51. Adh-Dhariyat" },
                new SelectListItem { Value = "At-Tur", Text = "52. At-Tur" },
                new SelectListItem { Value = "An-Najm", Text = "53. An-Najm" },
                new SelectListItem { Value = "Al-Qamar", Text = "54. Al-Qamar" },
                new SelectListItem { Value = "Ar-Rahman", Text = "55. Ar-Rahman" },
                new SelectListItem { Value = "Al-Waqi'ah", Text = "56. Al-Waqi'ah" },
                new SelectListItem { Value = "Al-Hadid", Text = "57. Al-Hadid" },
                new SelectListItem { Value = "Al-Mujadilah", Text = "58. Al-Mujadilah" },
                new SelectListItem { Value = "Al-Hashr", Text = "59. Al-Hashr" },
                new SelectListItem { Value = "Al-Mumtahanah", Text = "60. Al-Mumtahanah" },
                new SelectListItem { Value = "As-Saff", Text = "61. As-Saff" },
                new SelectListItem { Value = "Al-Jumu'ah", Text = "62. Al-Jumu'ah" },
                new SelectListItem { Value = "Al-Munafiqun", Text = "63. Al-Munafiqun" },
                new SelectListItem { Value = "At-Taghabun", Text = "64. At-Taghabun" },
                new SelectListItem { Value = "At-Talaq", Text = "65. At-Talaq" },
                new SelectListItem { Value = "At-Tahrim", Text = "66. At-Tahrim" },
                new SelectListItem { Value = "Al-Mulk", Text = "67. Al-Mulk" },
                new SelectListItem { Value = "Al-Qalam", Text = "68. Al-Qalam" },
                new SelectListItem { Value = "Al-Haqqah", Text = "69. Al-Haqqah" },
                new SelectListItem { Value = "Al-Ma'arij", Text = "70. Al-Ma'arij" },
                new SelectListItem { Value = "Nuh", Text = "71. Nuh" },
                new SelectListItem { Value = "Al-Jinn", Text = "72. Al-Jinn" },
                new SelectListItem { Value = "Al-Muzzammil", Text = "73. Al-Muzzammil" },
                new SelectListItem { Value = "Al-Muddaththir", Text = "74. Al-Muddaththir" },
                new SelectListItem { Value = "Al-Qiyamah", Text = "75. Al-Qiyamah" },
                new SelectListItem { Value = "Al-Insan", Text = "76. Al-Insan" },
                new SelectListItem { Value = "Al-Mursalat", Text = "77. Al-Mursalat" },
                new SelectListItem { Value = "An-Naba", Text = "78. An-Naba" },
                new SelectListItem { Value = "An-Nazi'at", Text = "79. An-Nazi'at" },
                new SelectListItem { Value = "Abasa", Text = "80. Abasa" },
                new SelectListItem { Value = "At-Takwir", Text = "81. At-Takwir" },
                new SelectListItem { Value = "Al-Infitar", Text = "82. Al-Infitar" },
                new SelectListItem { Value = "Al-Mutaffifin", Text = "83. Al-Mutaffifin" },
                new SelectListItem { Value = "Al-Inshiqaq", Text = "84. Al-Inshiqaq" },
                new SelectListItem { Value = "Al-Buruj", Text = "85. Al-Buruj" },
                new SelectListItem { Value = "At-Tariq", Text = "86. At-Tariq" },
                new SelectListItem { Value = "Al-A'la", Text = "87. Al-A'la" },
                new SelectListItem { Value = "Al-Ghashiyah", Text = "88. Al-Ghashiyah" },
                new SelectListItem { Value = "Al-Fajr", Text = "89. Al-Fajr" },
                new SelectListItem { Value = "Al-Balad", Text = "90. Al-Balad" },
                new SelectListItem { Value = "Ash-Shams", Text = "91. Ash-Shams" },
                new SelectListItem { Value = "Al-Layl", Text = "92. Al-Layl" },
                new SelectListItem { Value = "Ad-Duha", Text = "93. Ad-Duha" },
                new SelectListItem { Value = "Ash-Sharh", Text = "94. Ash-Sharh" },
                new SelectListItem { Value = "At-Tin", Text = "95. At-Tin" },
                new SelectListItem { Value = "Al-Alaq", Text = "96. Al-Alaq" },
                new SelectListItem { Value = "Al-Qadr", Text = "97. Al-Qadr" },
                new SelectListItem { Value = "Al-Bayyinah", Text = "98. Al-Bayyinah" },
                new SelectListItem { Value = "Az-Zalzalah", Text = "99. Az-Zalzalah" },
                new SelectListItem { Value = "Al-Adiyat", Text = "100. Al-Adiyat" },
                new SelectListItem { Value = "Al-Qari'ah", Text = "101. Al-Qari'ah" },
                new SelectListItem { Value = "At-Takathur", Text = "102. At-Takathur" },
                new SelectListItem { Value = "Al-Asr", Text = "103. Al-Asr" },
                new SelectListItem { Value = "Al-Humazah", Text = "104. Al-Humazah" },
                new SelectListItem { Value = "Al-Fil", Text = "105. Al-Fil" },
                new SelectListItem { Value = "Quraysh", Text = "106. Quraysh" },
                new SelectListItem { Value = "Al-Ma'un", Text = "107. Al-Ma'un" },
                new SelectListItem { Value = "Al-Kawthar", Text = "108. Al-Kawthar" },
                new SelectListItem { Value = "Al-Kafirun", Text = "109. Al-Kafirun" },
                new SelectListItem { Value = "An-Nasr", Text = "110. An-Nasr" },
                new SelectListItem { Value = "Al-Masad", Text = "111. Al-Masad" },
                new SelectListItem { Value = "Al-Ikhlas", Text = "112. Al-Ikhlas" },
                new SelectListItem { Value = "Al-Falaq", Text = "113. Al-Falaq" },
                new SelectListItem { Value = "An-Nas", Text = "114. An-Nas" }
            };
        }
    }
}