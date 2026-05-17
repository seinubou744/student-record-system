using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentRecordSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace StudentRecordSystem.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CreateTeacherModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateTeacherModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        public class InputModel
        {
            [Required]
            public string FullName { get; set; } = string.Empty;

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
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Input.FullName = Input.FullName?.Trim() ?? string.Empty;
            Input.Email = Input.Email?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(Input.FullName))
            {
                ModelState.AddModelError("Input.FullName", "Full name is required.");
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Input.Email))
            {
                ModelState.AddModelError("Input.Email", "Email is required.");
                return Page();
            }

            var existingEmailUser = await _userManager.FindByEmailAsync(Input.Email);
            if (existingEmailUser != null)
            {
                ModelState.AddModelError("Input.Email", "A user with this email already exists.");
                return Page();
            }

            var existingUserName = await _userManager.FindByNameAsync(Input.Email);
            if (existingUserName != null)
            {
                ModelState.AddModelError("Input.Email", "This email is already used as a login username.");
                return Page();
            }

            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                FullName = Input.FullName,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user, Input.Password);

            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return Page();
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "Teacher");

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return Page();
            }

            SuccessMessage = "Teacher account created successfully.";
            return RedirectToPage();
        }
    }
}