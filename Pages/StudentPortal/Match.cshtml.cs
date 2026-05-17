using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages.StudentPortal
{
    [Authorize(Policy = "StudentOnly")]
    public class MatchModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MatchModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty]
        public string? RequestMessage { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public Student? CurrentStudent { get; set; }

        public StudentLearningRecord? MyLatestQuran { get; set; }
        public StudentLearningRecord? MyLatestMutoon { get; set; }

        public List<QuranMatchViewModel> QuranMatches { get; set; } = new();
        public List<MutoonMatchViewModel> MutoonMatches { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            CurrentStudent = await _context.Students
                .AsNoTracking()
                .Include(s => s.ClassRoom)
                .FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id);

            if (CurrentStudent == null)
            {
                ErrorMessage = "Student profile was not found for the logged-in account.";
                return Page();
            }

            var studentId = CurrentStudent.Id;

            MyLatestQuran = await _context.StudentLearningRecords
                .AsNoTracking()
                .Where(q => q.StudentId == studentId && q.RecordType == LearningRecordType.Quran)
                .OrderByDescending(q => q.RecordDate)
                .ThenByDescending(q => q.Id)
                .FirstOrDefaultAsync();

            MyLatestMutoon = await _context.StudentLearningRecords
                .AsNoTracking()
                .Where(m => m.StudentId == studentId && m.RecordType == LearningRecordType.Mutoon)
                .OrderByDescending(m => m.RecordDate)
                .ThenByDescending(m => m.Id)
                .FirstOrDefaultAsync();

            if (MyLatestQuran != null && !string.IsNullOrWhiteSpace(MyLatestQuran.SurahName))
            {
                var quranQuery = _context.StudentLearningRecords
                    .AsNoTracking()
                    .Include(q => q.Student)
                    .ThenInclude(s => s.ClassRoom)
                    .Where(q =>
                        q.StudentId != studentId &&
                        q.RecordType == LearningRecordType.Quran &&
                        q.SurahName == MyLatestQuran.SurahName);

                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    quranQuery = quranQuery.Where(q =>
                        q.Student != null &&
                        q.Student.FullName.Contains(SearchTerm));
                }

                var quranResults = await quranQuery.ToListAsync();

                QuranMatches = quranResults
                    .Select(q => new QuranMatchViewModel
                    {
                        StudentId = q.StudentId,
                        StudentName = q.Student?.FullName ?? "",
                        ClassRoomName = q.Student?.ClassRoom?.Name ?? "",
                        SurahName = q.SurahName ?? "",
                        FromAyah = q.FromAyah ?? 0,
                        ToAyah = q.ToAyah ?? 0,
                        MatchScore = CalculateQuranMatchScore(MyLatestQuran, q),
                        MatchReason = BuildQuranMatchReason(MyLatestQuran, q)
                    })
                    .OrderByDescending(x => x.MatchScore)
                    .ThenBy(x => x.StudentName)
                    .ToList();
            }

            if (MyLatestMutoon != null && !string.IsNullOrWhiteSpace(MyLatestMutoon.Portion))
            {
                var mutoonQuery = _context.StudentLearningRecords
                    .AsNoTracking()
                    .Include(m => m.Student)
                    .ThenInclude(s => s.ClassRoom)
                    .Where(m =>
                        m.StudentId != studentId &&
                        m.RecordType == LearningRecordType.Mutoon &&
                        m.Portion != null);

                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    mutoonQuery = mutoonQuery.Where(m =>
                        m.Student != null &&
                        m.Student.FullName != null &&
                        m.Student.FullName.Contains(SearchTerm));
                }

                var mutoonResults = await mutoonQuery.ToListAsync();

                MutoonMatches = mutoonResults
                    .Select(m => new MutoonMatchViewModel
                    {
                        StudentId = m.StudentId,
                        StudentName = m.Student?.FullName ?? "",
                        ClassRoomName = m.Student?.ClassRoom?.Name ?? "",
                        Portion = m.Portion ?? "",
                        MatchScore = CalculateMutoonMatchScore(MyLatestMutoon, m),
                        MatchReason = BuildMutoonMatchReason(MyLatestMutoon, m)
                    })
                    .Where(x => x.MatchScore > 0)
                    .OrderByDescending(x => x.MatchScore)
                    .ThenBy(x => x.StudentName)
                    .ToList();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSendRequestAsync(int receiverStudentId, string requestType, string? searchTerm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var currentStudent = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id);

            if (currentStudent == null)
            {
                ErrorMessage = "Student profile was not found for the logged-in account.";
                return RedirectToPage(new { SearchTerm = searchTerm });
            }

            var senderStudentId = currentStudent.Id;

            if (senderStudentId == receiverStudentId)
            {
                ErrorMessage = "You cannot send a request to yourself.";
                return RedirectToPage(new { SearchTerm = searchTerm });
            }

            var receiverExists = await _context.Students
                .AsNoTracking()
                .AnyAsync(s => s.Id == receiverStudentId);

            if (!receiverExists)
            {
                ErrorMessage = "The selected student was not found.";
                return RedirectToPage(new { SearchTerm = searchTerm });
            }

            var validRequestType = requestType == "Quran" || requestType == "Mutoon";
            if (!validRequestType)
            {
                ErrorMessage = "Invalid request type.";
                return RedirectToPage(new { SearchTerm = searchTerm });
            }

            var pendingExists = await _context.PeerRequests.AnyAsync(r =>
                r.SenderStudentId == senderStudentId &&
                r.ReceiverStudentId == receiverStudentId &&
                r.RequestType == requestType &&
                r.Status == "Pending");

            if (pendingExists)
            {
                ErrorMessage = "You already sent a pending request to this student.";
                return RedirectToPage(new { SearchTerm = searchTerm });
            }

            var reversePendingExists = await _context.PeerRequests.AnyAsync(r =>
                r.SenderStudentId == receiverStudentId &&
                r.ReceiverStudentId == senderStudentId &&
                r.RequestType == requestType &&
                r.Status == "Pending");

            if (reversePendingExists)
            {
                ErrorMessage = "This student has already sent you a pending request.";
                return RedirectToPage(new { SearchTerm = searchTerm });
            }

            var peerRequest = new PeerRequest
            {
                SenderStudentId = senderStudentId,
                ReceiverStudentId = receiverStudentId,
                RequestType = requestType,
                Status = "Pending",
                Message = string.IsNullOrWhiteSpace(RequestMessage) ? null : RequestMessage.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.PeerRequests.Add(peerRequest);
            await _context.SaveChangesAsync();

            SuccessMessage = "Request sent successfully.";
            return RedirectToPage(new { SearchTerm = searchTerm });
        }

        private static int CalculateQuranMatchScore(StudentLearningRecord mine, StudentLearningRecord other)
        {
            var score = 0;

            if (!string.IsNullOrWhiteSpace(mine.SurahName) &&
                mine.SurahName == other.SurahName)
            {
                score += 70;
            }

            if (mine.FromAyah.HasValue && other.ToAyah.HasValue)
            {
                var ayahDifference = Math.Abs(mine.FromAyah.Value - other.ToAyah.Value);

                if (ayahDifference == 0)
                    score += 20;
                else if (ayahDifference <= 2)
                    score += 15;
                else if (ayahDifference <= 5)
                    score += 10;
                else if (ayahDifference <= 10)
                    score += 5;
            }

            return score;
        }

        private static int CalculateMutoonMatchScore(StudentLearningRecord mine, StudentLearningRecord other)
        {
            var score = 0;

            if (!string.IsNullOrWhiteSpace(mine.Portion) &&
                !string.IsNullOrWhiteSpace(other.Portion))
            {
                if (mine.Portion.Trim().Equals(other.Portion.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    score += 100;
                }
                else if (other.Portion.Contains(mine.Portion, StringComparison.OrdinalIgnoreCase) ||
                         mine.Portion.Contains(other.Portion, StringComparison.OrdinalIgnoreCase))
                {
                    score += 60;
                }
            }

            return score;
        }

        private static string BuildQuranMatchReason(StudentLearningRecord mine, StudentLearningRecord other)
        {
            if (!string.IsNullOrWhiteSpace(mine.SurahName) &&
                mine.SurahName == other.SurahName)
            {
                if (mine.FromAyah.HasValue && other.ToAyah.HasValue &&
                    mine.FromAyah.Value == other.ToAyah.Value)
                {
                    return "Same surah and very close ayah progress.";
                }

                return "Same surah with close Quran progress.";
            }

            return "General Quran match.";
        }

        private static string BuildMutoonMatchReason(StudentLearningRecord mine, StudentLearningRecord other)
        {
            if (!string.IsNullOrWhiteSpace(mine.Portion) &&
                !string.IsNullOrWhiteSpace(other.Portion))
            {
                if (mine.Portion.Trim().Equals(other.Portion.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return "Same Mutoon portion.";
                }

                if (other.Portion.Contains(mine.Portion, StringComparison.OrdinalIgnoreCase) ||
                    mine.Portion.Contains(other.Portion, StringComparison.OrdinalIgnoreCase))
                {
                    return "Very similar Mutoon portion.";
                }
            }

            return "General Mutoon match.";
        }

        public class QuranMatchViewModel
        {
            public int StudentId { get; set; }
            public string StudentName { get; set; } = "";
            public string ClassRoomName { get; set; } = "";
            public string SurahName { get; set; } = "";
            public int FromAyah { get; set; }
            public int ToAyah { get; set; }
            public int MatchScore { get; set; }
            public string MatchReason { get; set; } = "";
        }

        public class MutoonMatchViewModel
        {
            public int StudentId { get; set; }
            public string StudentName { get; set; } = "";
            public string ClassRoomName { get; set; } = "";
            public string Portion { get; set; } = "";
            public int MatchScore { get; set; }
            public string MatchReason { get; set; } = "";
        }
    }
}