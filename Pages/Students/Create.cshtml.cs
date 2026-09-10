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
        public Student Student { get; set; } = new();

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public string ConfirmPassword { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            LoadDropdowns();
            return Page();
        }

        public IActionResult OnPostLoadSections()
        {
            LoadDropdowns();
            ModelState.Clear();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            LoadDropdowns();

            Student.AdmissionNo = Student.AdmissionNo?.Trim() ?? string.Empty;
            Student.FullName = Student.FullName?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(Student.AdmissionNo))
                ModelState.AddModelError("Student.AdmissionNo", "Admission number is required.");

            if (string.IsNullOrWhiteSpace(Student.FullName))
                ModelState.AddModelError("Student.FullName", "Student name is required.");

            if (Student.ClassRoomId <= 0)
                ModelState.AddModelError("Student.ClassRoomId", "Class is required.");

            if (Student.SectionId <= 0)
                ModelState.AddModelError("Student.SectionId", "Section is required.");

            if (Student.ClassRoomId > 0 && Student.SectionId > 0)
            {
                var validSection = await _context.Sections
                    .AnyAsync(s => s.Id == Student.SectionId && s.ClassRoomId == Student.ClassRoomId);

                if (!validSection)
                    ModelState.AddModelError("Student.SectionId", "Selected section does not belong to the selected class.");
            }

            if (string.IsNullOrWhiteSpace(Password))
                ModelState.AddModelError("Password", "Password is required.");

            if (Password != ConfirmPassword)
                ModelState.AddModelError("ConfirmPassword", "Password and confirmation password do not match.");

            var admissionExists = await _context.Students
                .AnyAsync(s => s.AdmissionNo == Student.AdmissionNo);

            if (admissionExists)
                ModelState.AddModelError("Student.AdmissionNo", "This admission number already exists.");

            var existingUser = await _userManager.FindByNameAsync(Student.AdmissionNo);
            if (existingUser != null)
                ModelState.AddModelError("Student.AdmissionNo", "This admission number is already used as a login username.");

            if (!ModelState.IsValid)
                return Page();

            using var transaction = await _context.Database.BeginTransactionAsync();
            ApplicationUser? studentUser = null;

            try
            {
                studentUser = new ApplicationUser
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
                        ModelState.AddModelError(string.Empty, error.Description);

                    await transaction.RollbackAsync();
                    LoadDropdowns();
                    return Page();
                }

                var addToRoleResult = await _userManager.AddToRoleAsync(studentUser, "Student");

                if (!addToRoleResult.Succeeded)
                {
                    foreach (var error in addToRoleResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);

                    await transaction.RollbackAsync();

                    var createdUser = await _userManager.FindByIdAsync(studentUser.Id);
                    if (createdUser != null)
                        await _userManager.DeleteAsync(createdUser);

                    LoadDropdowns();
                    return Page();
                }

                Student.ApplicationUserId = studentUser.Id;

                _context.Students.Add(Student);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["SuccessMessage"] = $"Student created successfully. Login username is the admission number: {Student.AdmissionNo}";
                return RedirectToPage("./Index");
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();

                if (studentUser != null)
                {
                    var createdUser = await _userManager.FindByIdAsync(studentUser.Id);
                    if (createdUser != null)
                        await _userManager.DeleteAsync(createdUser);
                }

                ModelState.AddModelError(string.Empty, "Database error while saving student.");
                ModelState.AddModelError(string.Empty, ex.InnerException?.Message ?? ex.Message);

                LoadDropdowns();
                return Page();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                if (studentUser != null)
                {
                    var createdUser = await _userManager.FindByIdAsync(studentUser.Id);
                    if (createdUser != null)
                        await _userManager.DeleteAsync(createdUser);
                }

                ModelState.AddModelError(string.Empty, ex.Message);

                LoadDropdowns();
                return Page();
            }
        }

        private void LoadDropdowns()
        {
            ViewData["ClassRoomId"] = new SelectList(
                _context.ClassRooms.OrderBy(c => c.Name).ToList(),
                "Id",
                "Name",
                Student?.ClassRoomId);

            var sectionsQuery = _context.Sections.AsQueryable();

            if (Student?.ClassRoomId > 0)
            {
                sectionsQuery = sectionsQuery.Where(s => s.ClassRoomId == Student.ClassRoomId);
            }

            ViewData["SectionId"] = new SelectList(
                sectionsQuery.OrderBy(s => s.Name).ToList(),
                "Id",
                "Name",
                Student?.SectionId);
        }
    }
}