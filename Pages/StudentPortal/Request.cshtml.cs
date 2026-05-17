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
    public class RequestsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RequestsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<PeerRequestViewModel> IncomingRequests { get; set; } = new();
        public List<PeerRequestViewModel> SentRequests { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var studentId = await GetCurrentStudentIdAsync();
            if (studentId == null)
            {
                ErrorMessage = "Student profile was not found for the logged-in account.";
                return Page();
            }

            await LoadRequestsAsync(studentId.Value);
            return Page();
        }

        public async Task<IActionResult> OnPostAcceptAsync(int id)
        {
            var studentId = await GetCurrentStudentIdAsync();
            if (studentId == null)
            {
                ErrorMessage = "Student profile was not found for the logged-in account.";
                return RedirectToPage();
            }

            var request = await _context.PeerRequests
                .FirstOrDefaultAsync(r => r.Id == id && r.ReceiverStudentId == studentId.Value);

            if (request == null)
            {
                ErrorMessage = "Request not found.";
                return RedirectToPage();
            }

            if (request.Status != "Pending")
            {
                ErrorMessage = "Only pending requests can be accepted.";
                return RedirectToPage();
            }

            request.Status = "Accepted";
            await _context.SaveChangesAsync();

            SuccessMessage = "Request accepted successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            var studentId = await GetCurrentStudentIdAsync();
            if (studentId == null)
            {
                ErrorMessage = "Student profile was not found for the logged-in account.";
                return RedirectToPage();
            }

            var request = await _context.PeerRequests
                .FirstOrDefaultAsync(r => r.Id == id && r.ReceiverStudentId == studentId.Value);

            if (request == null)
            {
                ErrorMessage = "Request not found.";
                return RedirectToPage();
            }

            if (request.Status != "Pending")
            {
                ErrorMessage = "Only pending requests can be rejected.";
                return RedirectToPage();
            }

            request.Status = "Rejected";
            await _context.SaveChangesAsync();

            SuccessMessage = "Request rejected successfully.";
            return RedirectToPage();
        }

        private async Task<int?> GetCurrentStudentIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return null;
            }

            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id);

            return student?.Id;
        }

        private async Task LoadRequestsAsync(int studentId)
        {
            var incoming = await _context.PeerRequests
                .AsNoTracking()
                .Include(r => r.SenderStudent)
                    .ThenInclude(s => s.ClassRoom)
                .Where(r => r.ReceiverStudentId == studentId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            IncomingRequests = incoming.Select(r => new PeerRequestViewModel
            {
                Id = r.Id,
                OtherStudentName = r.SenderStudent?.FullName ?? "",
                OtherStudentClassRoom = r.SenderStudent?.ClassRoom?.Name ?? "",
                RequestType = r.RequestType,
                Status = r.Status,
                Message = r.Message,
                CreatedAt = r.CreatedAt,
                IsIncoming = true
            }).ToList();

            var sent = await _context.PeerRequests
                .AsNoTracking()
                .Include(r => r.ReceiverStudent)
                    .ThenInclude(s => s.ClassRoom)
                .Where(r => r.SenderStudentId == studentId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            SentRequests = sent.Select(r => new PeerRequestViewModel
            {
                Id = r.Id,
                OtherStudentName = r.ReceiverStudent?.FullName ?? "",
                OtherStudentClassRoom = r.ReceiverStudent?.ClassRoom?.Name ?? "",
                RequestType = r.RequestType,
                Status = r.Status,
                Message = r.Message,
                CreatedAt = r.CreatedAt,
                IsIncoming = false
            }).ToList();
        }

        public class PeerRequestViewModel
        {
            public int Id { get; set; }
            public string OtherStudentName { get; set; } = "";
            public string OtherStudentClassRoom { get; set; } = "";
            public string RequestType { get; set; } = "";
            public string Status { get; set; } = "";
            public string? Message { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool IsIncoming { get; set; }
        }
    }
}