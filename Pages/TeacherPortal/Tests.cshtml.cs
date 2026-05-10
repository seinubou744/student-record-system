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
    public class TestsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TestsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public int? ClassRoomId { get; set; }

        [BindProperty(SupportsGet = true)]
        public LearningRecordType TestType { get; set; } = LearningRecordType.Quran;

        [BindProperty(SupportsGet = true)]
        public DateTime TestDate { get; set; } = DateTime.UtcNow.Date;

        [BindProperty]
        public List<TestInputModel> Items { get; set; } = new();

        public List<SelectListItem> TeacherClasses { get; set; } = new();
        public List<SelectListItem> SurahOptions { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        public string? DebugMessage { get; set; }

        public class TestInputModel
        {
            public int StudentId { get; set; }
            public string StudentCode { get; set; } = string.Empty;
            public string StudentName { get; set; } = string.Empty;
            public int ClassRoomId { get; set; }

            public string? SurahName { get; set; }
            public int? FromAyah { get; set; }
            public int? ToAyah { get; set; }

            public string? Portion { get; set; }
            public decimal Marks { get; set; }
            public string? Remarks { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            LoadSurahOptions();
            await LoadPageAsync();
            return Page();
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
                DebugMessage = "POST failed: ClassRoomId is null.";
                await LoadPageAsync();
                return Page();
            }

            var allowed = await _context.TeacherClasses
                .AnyAsync(tc => tc.TeacherUserId == user.Id && tc.ClassRoomId == ClassRoomId.Value);

            if (!allowed)
            {
                DebugMessage = $"POST failed: Teacher {user.Id} is not assigned to class {ClassRoomId.Value}.";
                return Forbid();
            }

            if (TestDate == default)
            {
                ModelState.AddModelError(string.Empty, "Please enter a valid test date.");
                DebugMessage = "POST failed: TestDate is invalid.";
                await LoadPageAsync();
                return Page();
            }

            if (!Items.Any())
            {
                ModelState.AddModelError(string.Empty, "No students were loaded for this class.");
                DebugMessage = $"POST failed: No Items were submitted for class {ClassRoomId.Value}.";
                await LoadPageAsync();
                return Page();
            }

            var rowsToSave = new List<TestInputModel>();

            foreach (var item in Items)
            {
                bool hasAnyData =
                    !string.IsNullOrWhiteSpace(item.SurahName) ||
                    item.FromAyah.HasValue ||
                    item.ToAyah.HasValue ||
                    !string.IsNullOrWhiteSpace(item.Portion) ||
                    item.Marks > 0 ||
                    !string.IsNullOrWhiteSpace(item.Remarks);

                if (!hasAnyData)
                {
                    continue;
                }

                if (TestType == LearningRecordType.Quran)
                {
                    if (string.IsNullOrWhiteSpace(item.SurahName))
                    {
                        ModelState.AddModelError(string.Empty, $"Please select a Surah for {item.StudentName}.");
                    }

                    if (!item.FromAyah.HasValue)
                    {
                        ModelState.AddModelError(string.Empty, $"Please select From Ayah for {item.StudentName}.");
                    }

                    if (!item.ToAyah.HasValue)
                    {
                        ModelState.AddModelError(string.Empty, $"Please select To Ayah for {item.StudentName}.");
                    }

                    if (item.FromAyah.HasValue && item.ToAyah.HasValue &&
                        item.FromAyah.Value > item.ToAyah.Value)
                    {
                        ModelState.AddModelError(string.Empty, $"From Ayah cannot be greater than To Ayah for {item.StudentName}.");
                    }
                }
                else if (TestType == LearningRecordType.Mutoon)
                {
                    if (string.IsNullOrWhiteSpace(item.Portion))
                    {
                        ModelState.AddModelError(string.Empty, $"Please enter a portion for {item.StudentName}.");
                    }
                }

                rowsToSave.Add(item);
            }

            if (!rowsToSave.Any())
            {
                ModelState.AddModelError(string.Empty, "Please complete at least one student test before saving.");
                DebugMessage = "POST failed: No completed student rows found.";
                await LoadPageAsync();
                return Page();
            }

            if (!ModelState.IsValid)
            {
                DebugMessage = "POST failed: Validation errors exist.";
                await LoadPageAsync();
                return Page();
            }

            var selectedDate = DateTime.SpecifyKind(TestDate.Date, DateTimeKind.Utc);

            var existing = await _context.StudentTestRecords
                .Where(t => t.ClassRoomId == ClassRoomId.Value &&
                            t.TestDate == selectedDate &&
                            t.TestType == TestType)
                .ToListAsync();

            foreach (var item in rowsToSave)
            {
                var saved = existing.FirstOrDefault(x => x.StudentId == item.StudentId);

                if (saved == null)
                {
                    _context.StudentTestRecords.Add(new StudentTestRecord
                    {
                        StudentId = item.StudentId,
                        ClassRoomId = item.ClassRoomId,
                        TestType = TestType,
                        TestDate = selectedDate,
                        SurahName = TestType == LearningRecordType.Quran ? item.SurahName : null,
                        FromAyah = TestType == LearningRecordType.Quran ? item.FromAyah : null,
                        ToAyah = TestType == LearningRecordType.Quran ? item.ToAyah : null,
                        Portion = TestType == LearningRecordType.Mutoon ? item.Portion : null,
                        Marks = item.Marks,
                        Remarks = item.Remarks,
                        TeacherUserId = user.Id
                    });
                }
                else
                {
                    saved.SurahName = TestType == LearningRecordType.Quran ? item.SurahName : null;
                    saved.FromAyah = TestType == LearningRecordType.Quran ? item.FromAyah : null;
                    saved.ToAyah = TestType == LearningRecordType.Quran ? item.ToAyah : null;
                    saved.Portion = TestType == LearningRecordType.Mutoon ? item.Portion : null;
                    saved.Marks = item.Marks;
                    saved.Remarks = item.Remarks;
                    saved.TeacherUserId = user.Id;
                    saved.TestDate = selectedDate;
                }
            }

            await _context.SaveChangesAsync();
            SuccessMessage = $"{rowsToSave.Count} {TestType} test record(s) saved successfully.";

            return RedirectToPage(new
            {
                ClassRoomId,
                TestType,
                TestDate = selectedDate.ToString("yyyy-MM-dd")
            });
        }

        private async Task LoadPageAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                DebugMessage = "No logged in teacher found.";
                return;
            }

            LoadSurahOptions();
            await LoadTeacherClassesAsync(user.Id);

            if (!TeacherClasses.Any())
            {
                DebugMessage = $"No classes assigned to teacher {user.Id}.";
                return;
            }

            if (ClassRoomId == null)
            {
                ClassRoomId = int.Parse(TeacherClasses.First().Value);
            }

            var isAssigned = TeacherClasses.Any(c => c.Value == ClassRoomId.Value.ToString());
            if (!isAssigned)
            {
                DebugMessage = $"Selected class {ClassRoomId.Value} is not in the teacher assigned classes list.";
                Items = new List<TestInputModel>();
                return;
            }

            var selectedDate = DateTime.SpecifyKind(TestDate.Date, DateTimeKind.Utc);

            Items = await _context.Students
                .Where(s => s.ClassRoomId == ClassRoomId.Value)
                .OrderBy(s => s.FullName)
                .Select(s => new TestInputModel
                {
                    StudentId = s.Id,
                    StudentCode = s.AdmissionNo,
                    StudentName = s.FullName,
                    ClassRoomId = s.ClassRoomId
                })
                .ToListAsync();

            if (!Items.Any())
            {
                DebugMessage = $"No students found in class {ClassRoomId.Value}.";
                return;
            }

            var existing = await _context.StudentTestRecords
                .Where(t => t.ClassRoomId == ClassRoomId.Value &&
                            t.TestDate == selectedDate &&
                            t.TestType == TestType)
                .ToListAsync();

            foreach (var item in Items)
            {
                var saved = existing.FirstOrDefault(x => x.StudentId == item.StudentId);
                if (saved != null)
                {
                    item.SurahName = saved.SurahName;
                    item.FromAyah = saved.FromAyah;
                    item.ToAyah = saved.ToAyah;
                    item.Portion = saved.Portion;
                    item.Marks = saved.Marks;
                    item.Remarks = saved.Remarks;
                }
            }

            DebugMessage = $"Loaded {Items.Count} students for class {ClassRoomId.Value}.";
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
            var surahs = new List<string>
            {
                "Al-Fatihah",
                "Al-Baqarah",
                "Aal-E-Imran",
                "An-Nisa",
                "Al-Ma'idah",
                "Al-An'am",
                "Al-A'raf",
                "Al-Anfal",
                "At-Tawbah",
                "Yunus",
                "Hud",
                "Yusuf",
                "Ar-Ra'd",
                "Ibrahim",
                "Al-Hijr",
                "An-Nahl",
                "Al-Isra",
                "Al-Kahf",
                "Maryam",
                "Ta-Ha",
                "Al-Anbiya",
                "Al-Hajj",
                "Al-Mu'minun",
                "An-Nur",
                "Al-Furqan",
                "Ash-Shu'ara",
                "An-Naml",
                "Al-Qasas",
                "Al-Ankabut",
                "Ar-Rum",
                "Luqman",
                "As-Sajdah",
                "Al-Ahzab",
                "Saba",
                "Fatir",
                "Ya-Sin",
                "As-Saffat",
                "Sad",
                "Az-Zumar",
                "Ghafir",
                "Fussilat",
                "Ash-Shura",
                "Az-Zukhruf",
                "Ad-Dukhan",
                "Al-Jathiyah",
                "Al-Ahqaf",
                "Muhammad",
                "Al-Fath",
                "Al-Hujurat",
                "Qaf",
                "Adh-Dhariyat",
                "At-Tur",
                "An-Najm",
                "Al-Qamar",
                "Ar-Rahman",
                "Al-Waqi'ah",
                "Al-Hadid",
                "Al-Mujadilah",
                "Al-Hashr",
                "Al-Mumtahanah",
                "As-Saff",
                "Al-Jumu'ah",
                "Al-Munafiqun",
                "At-Taghabun",
                "At-Talaq",
                "At-Tahrim",
                "Al-Mulk",
                "Al-Qalam",
                "Al-Haqqah",
                "Al-Ma'arij",
                "Nuh",
                "Al-Jinn",
                "Al-Muzzammil",
                "Al-Muddaththir",
                "Al-Qiyamah",
                "Al-Insan",
                "Al-Mursalat",
                "An-Naba",
                "An-Nazi'at",
                "Abasa",
                "At-Takwir",
                "Al-Infitar",
                "Al-Mutaffifin",
                "Al-Inshiqaq",
                "Al-Buruj",
                "At-Tariq",
                "Al-A'la",
                "Al-Ghashiyah",
                "Al-Fajr",
                "Al-Balad",
                "Ash-Shams",
                "Al-Layl",
                "Ad-Duha",
                "Ash-Sharh",
                "At-Tin",
                "Al-Alaq",
                "Al-Qadr",
                "Al-Bayyinah",
                "Az-Zalzalah",
                "Al-Adiyat",
                "Al-Qari'ah",
                "At-Takathur",
                "Al-Asr",
                "Al-Humazah",
                "Al-Fil",
                "Quraysh",
                "Al-Ma'un",
                "Al-Kawthar",
                "Al-Kafirun",
                "An-Nasr",
                "Al-Masad",
                "Al-Ikhlas",
                "Al-Falaq",
                "An-Nas"
            };

            SurahOptions = surahs
                .Select(s => new SelectListItem
                {
                    Value = s,
                    Text = s
                })
                .ToList();
        }
    }
}