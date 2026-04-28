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
    public class GenerateLoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public GenerateLoginModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public Student Student { get; set; } = default!;

        [TempData]
        public string? GeneratedEmail { get; set; }

        [TempData]
        public string? GeneratedPassword { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

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

            if (!string.IsNullOrEmpty(student.ApplicationUserId))
            {
                TempData["SuccessMessage"] = "This student already has a login account.";
                return RedirectToPage("./Index");
            }

            if (!await _roleManager.RoleExistsAsync("Student"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Student"));
            }

            var baseEmail = $"{student.AdmissionNo.ToLower()}@student.local";
            var email = baseEmail;
            var counter = 1;

            while (await _userManager.FindByEmailAsync(email) != null)
            {
                email = $"{student.AdmissionNo.ToLower()}{counter}@student.local";
                counter++;
            }

            var password = GenerateTemporaryPassword(student.AdmissionNo);

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = student.FullName,
                StudentId = student.Id
            };

            var result = await _userManager.CreateAsync(user, password);

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

            GeneratedEmail = email;
            GeneratedPassword = password;
            SuccessMessage = $"Login account created successfully for {student.FullName}.";
            TempData["SuccessMessage"] = SuccessMessage;

            return RedirectToPage("./GeneratedLoginResult", new { id = student.Id });
        }

        private string GenerateTemporaryPassword(string admissionNo)
        {
            var cleanAdmissionNo = admissionNo.Replace(" ", "");
            return $"{cleanAdmissionNo}@123Aa";
        }
    }
}