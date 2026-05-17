using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Pages_Students
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Student Student { get; set; } = default!;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public string ConfirmPassword { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            LoadClassRooms();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            LoadClassRooms();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            Student.AdmissionNo = Student.AdmissionNo?.Trim() ?? string.Empty;
            Student.FullName = Student.FullName?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(Student.AdmissionNo))
            {
                ModelState.AddModelError("Student.AdmissionNo", "Admission number is required.");
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Student.FullName))
            {
                ModelState.AddModelError("Student.FullName", "Student name is required.");
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ModelState.AddModelError("Password", "Password is required.");
                return Page();
            }

            if (Password != ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Password and confirmation password do not match.");
                return Page();
            }

            var admissionExists = await _context.Students
                .AnyAsync(s => s.AdmissionNo == Student.AdmissionNo);

            if (admissionExists)
            {
                ModelState.AddModelError("Student.AdmissionNo", "This admission number already exists.");
                return Page();
            }

            var existingUser = await _userManager.FindByNameAsync(Student.AdmissionNo);
            if (existingUser != null)
            {
                ModelState.AddModelError("Student.AdmissionNo", "This admission number is already used as a login username.");
                return Page();
            }

            var studentUser = new ApplicationUser
            {
                UserName = Student.AdmissionNo,
                Email = $"{Student.AdmissionNo}@student.local",
                EmailConfirmed = true,
                FullName = Student.FullName
            };

            var createUserResult = await _userManager.CreateAsync(studentUser, Password);

            if (!createUserResult.Succeeded)
            {
                foreach (var error in createUserResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return Page();
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(studentUser, "Student");

            if (!addToRoleResult.Succeeded)
            {
                await _userManager.DeleteAsync(studentUser);

                foreach (var error in addToRoleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return Page();
            }

            Student.ApplicationUserId = studentUser.Id;

            _context.Students.Add(Student);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Student created successfully. Login username is the admission number: {Student.AdmissionNo}";
            return RedirectToPage("./Index");
        }

        private void LoadClassRooms()
        {
            ViewData["ClassRoomId"] = new SelectList(_context.ClassRooms.OrderBy(c => c.Name), "Id", "Name");
        }
    }
}