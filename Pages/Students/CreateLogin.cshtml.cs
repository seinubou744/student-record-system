using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages_Students
{
    [Authorize(Roles = "Admin")]
    public class CreateLoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateLoginModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Student Student { get; set; } = default!;

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            [StringLength(100, MinimumLength = 6)]
            public string Password { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            [Compare("Password")]
            [Display(Name = "Confirm Password")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var student = await _context.Students
                .Include(s => s.ClassRoom)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            Student = student;

            if (string.IsNullOrWhiteSpace(Input.Email))
            {
                Input.Email = $"{student.AdmissionNo}@student.local";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var student = await _context.Students
                .Include(s => s.ClassRoom)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            Student = student;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!string.IsNullOrEmpty(student.ApplicationUserId))
            {
                ModelState.AddModelError(string.Empty, "This student already has a linked login account.");
                return Page();
            }

            var existingUser = await _userManager.FindByEmailAsync(Input.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "This email is already used by another account.");
                return Page();
            }

            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                EmailConfirmed = true,
                FullName = student.FullName,
                StudentId = student.Id
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            await _userManager.AddToRoleAsync(user, "Student");

            student.ApplicationUserId = user.Id;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Student login account created successfully.";
            return RedirectToPage("./Details", new { id = student.Id });
        }
    }
}