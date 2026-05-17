using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages.Admin
{
	[Authorize(Roles = "Admin")]
	public class ResetUserPasswordModel : PageModel
	{
		private readonly UserManager<ApplicationUser> _userManager;

		public ResetUserPasswordModel(UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
		}

		[BindProperty]
		public InputModel Input { get; set; } = new();

		public string TargetUserDisplayName { get; set; } = string.Empty;
		public string TargetUserLogin { get; set; } = string.Empty;

		[TempData]
		public string? SuccessMessage { get; set; }

		public class InputModel
		{
			[Required]
			public string UserId { get; set; } = string.Empty;

			[Required]
			[Display(Name = "Full Name")]
			public string FullName { get; set; } = string.Empty;

			[Required]
			[Display(Name = "Login Name")]
			public string UserName { get; set; } = string.Empty;

			[Required]
			[DataType(DataType.Password)]
			[StringLength(100, MinimumLength = 6, ErrorMessage = "The password must be at least 6 characters long.")]
			[Display(Name = "New Password")]
			public string NewPassword { get; set; } = string.Empty;

			[Required]
			[DataType(DataType.Password)]
			[Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
			[Display(Name = "Confirm Password")]
			public string ConfirmPassword { get; set; } = string.Empty;
		}

		public async Task<IActionResult> OnGetAsync(string? userId)
		{
			if (string.IsNullOrWhiteSpace(userId))
			{
				return NotFound();
			}

			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return NotFound();
			}

			Input = new InputModel
			{
				UserId = user.Id,
				FullName = user.FullName ?? string.Empty,
				UserName = user.UserName ?? string.Empty
			};

			TargetUserDisplayName = user.FullName ?? string.Empty;
			TargetUserLogin = user.UserName ?? string.Empty;

			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (string.IsNullOrWhiteSpace(Input.UserId))
			{
				return NotFound();
			}

			var user = await _userManager.FindByIdAsync(Input.UserId);
			if (user == null)
			{
				return NotFound();
			}

			TargetUserDisplayName = user.FullName ?? string.Empty;
			TargetUserLogin = user.UserName ?? string.Empty;

			if (!ModelState.IsValid)
			{
				return Page();
			}

			var token = await _userManager.GeneratePasswordResetTokenAsync(user);
			var result = await _userManager.ResetPasswordAsync(user, token, Input.NewPassword);

			if (!result.Succeeded)
			{
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}

				return Page();
			}

			SuccessMessage = $"Password reset successfully for {user.FullName ?? user.UserName}.";
			return RedirectToPage();
		}
	}
}