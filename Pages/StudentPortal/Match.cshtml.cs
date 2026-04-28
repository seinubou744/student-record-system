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

        public QuranRecord? MyLatestQuran { get; set; }
        public MutoonRecord? MyLatestMutoon { get; set; }

        public List<QuranMatchViewModel> QuranMatches { get; set; } = new();
        public List<MutoonMatchViewModel> MutoonMatches { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.StudentId == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var studentId = user.StudentId.Value;

            CurrentStudent = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (CurrentStudent == null)
            {
                return Page();
            }

            MyLatestQuran = await _context.QuranRecords
                .AsNoTracking()
                .Where(q => q.StudentId == studentId)
                .OrderByDescending(q => q.Id)
                .FirstOrDefaultAsync();

            MyLatestMutoon = await _context.MutoonRecords
                .AsNoTracking()
                .Where(m => m.StudentId == studentId)
                .OrderByDescending(m => m.Id)
                .FirstOrDefaultAsync();

            if (MyLatestQuran != null)
            {
                var quranQuery = _context.QuranRecords
                    .AsNoTracking()
                    .Include(q => q.Student)
                    .ThenInclude(s => s.ClassRoom)
                    .Where(q => q.StudentId != studentId && q.SurahName == MyLatestQuran.SurahName);

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
                        SurahName = q.SurahName,
                        PageNumber = q.PageNumber,
                        FromAyah = q.FromAyah,
                        ToAyah = q.ToAyah,
                        MatchScore = CalculateQuranMatchScore(MyLatestQuran, q),
                        MatchReason = BuildQuranMatchReason(MyLatestQuran, q)
                    })
                    .OrderByDescending(x => x.MatchScore)
                    .ThenBy(x => x.StudentName)
                    .ToList();
            }

            if (MyLatestMutoon != null)
            {
                var mutoonQuery = _context.MutoonRecords
                    .AsNoTracking()
                    .Include(m => m.Student)
                    .ThenInclude(s => s.ClassRoom)
                    .Where(m => m.StudentId != studentId && m.MatnName == MyLatestMutoon.MatnName);

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
                        MatnName = m.MatnName ?? "",
                        Portion = m.Portion ?? "",
                        MatchScore = CalculateMutoonMatchScore(MyLatestMutoon, m),
                        MatchReason = BuildMutoonMatchReason(MyLatestMutoon, m)
                    })
                    .OrderByDescending(x => x.MatchScore)
                    .ThenBy(x => x.StudentName)
                    .ToList();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSendRequestAsync(int receiverStudentId, string requestType, string? searchTerm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.StudentId == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var senderStudentId = user.StudentId.Value;

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

        private static int CalculateQuranMatchScore(QuranRecord mine, QuranRecord other)
        {
            var score = 0;

            if (mine.SurahName == other.SurahName)
                score += 70;

            var pageDifference = Math.Abs(mine.PageNumber - other.PageNumber);
            if (pageDifference == 0)
                score += 20;
            else if (pageDifference <= 1)
                score += 15;
            else if (pageDifference <= 3)
                score += 10;
            else if (pageDifference <= 5)
                score += 5;

            var ayahDifference = Math.Abs(mine.FromAyah - other.ToAyah);
            if (ayahDifference == 0)
                score += 10;
            else if (ayahDifference <= 2)
                score += 7;
            else if (ayahDifference <= 5)
                score += 4;

            return score;
        }

        private static int CalculateMutoonMatchScore(MutoonRecord mine, MutoonRecord other)
        {
            var score = 0;

            if (!string.IsNullOrWhiteSpace(mine.MatnName) &&
                mine.MatnName == other.MatnName)
            {
                score += 80;
            }

            if (!string.IsNullOrWhiteSpace(mine.Portion) &&
                !string.IsNullOrWhiteSpace(other.Portion))
            {
                if (mine.Portion.Trim().Equals(other.Portion.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    score += 20;
                }
                else if (other.Portion.Contains(mine.Portion, StringComparison.OrdinalIgnoreCase) ||
                         mine.Portion.Contains(other.Portion, StringComparison.OrdinalIgnoreCase))
                {
                    score += 10;
                }
            }

            return score;
        }

        private static string BuildQuranMatchReason(QuranRecord mine, QuranRecord other)
        {
            if (mine.SurahName == other.SurahName && mine.PageNumber == other.PageNumber && mine.FromAyah == other.ToAyah)
                return "Same surah, same page, and same ayah.";

            if (mine.SurahName == other.SurahName && mine.PageNumber == other.PageNumber)
                return "Same surah and same page.";

            if (mine.SurahName == other.SurahName)
                return "Same surah with close progress.";

            return "General Quran match.";
        }

        private static string BuildMutoonMatchReason(MutoonRecord mine, MutoonRecord other)
        {
            if (!string.IsNullOrWhiteSpace(mine.MatnName) &&
                mine.MatnName == other.MatnName &&
                !string.IsNullOrWhiteSpace(mine.Portion) &&
                mine.Portion.Trim().Equals(other.Portion?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return "Same matn and same portion.";
            }

            if (!string.IsNullOrWhiteSpace(mine.MatnName) &&
                mine.MatnName == other.MatnName)
            {
                return "Same matn with close text progress.";
            }

            return "General Mutoon match.";
        }

        public class QuranMatchViewModel
        {
            public int StudentId { get; set; }
            public string StudentName { get; set; } = "";
            public string ClassRoomName { get; set; } = "";
            public string SurahName { get; set; } = "";
            public int PageNumber { get; set; }
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
            public string MatnName { get; set; } = "";
            public string Portion { get; set; } = "";
            public int MatchScore { get; set; }
            public string MatchReason { get; set; } = "";
        }
    }
}