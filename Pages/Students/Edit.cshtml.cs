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
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EditModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Student Student { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            Student = student;
            LoadClassRooms(student.ClassRoomId);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            LoadClassRooms(Student.ClassRoomId);

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
                ModelState.AddModelError("Student.FullName", "Student full name is required.");
                return Page();
            }

            var existingStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == Student.Id);

            if (existingStudent == null)
            {
                return NotFound();
            }

            var duplicateAdmission = await _context.Students
                .AnyAsync(s => s.Id != Student.Id && s.AdmissionNo == Student.AdmissionNo);

            if (duplicateAdmission)
            {
                ModelState.AddModelError("Student.AdmissionNo", "This admission number already exists.");
                return Page();
            }

            existingStudent.AdmissionNo = Student.AdmissionNo;
            existingStudent.FullName = Student.FullName;
            existingStudent.ClassRoomId = Student.ClassRoomId;

            if (!string.IsNullOrWhiteSpace(existingStudent.ApplicationUserId))
            {
                var user = await _userManager.FindByIdAsync(existingStudent.ApplicationUserId);

                if (user != null)
                {
                    var duplicateUserName = await _userManager.FindByNameAsync(Student.AdmissionNo);

                    if (duplicateUserName != null && duplicateUserName.Id != user.Id)
                    {
                        ModelState.AddModelError("Student.AdmissionNo", "This admission number is already used as a login username.");
                        return Page();
                    }

                    user.UserName = Student.AdmissionNo;
                    user.Email = $"{Student.AdmissionNo}@student.local";
                    user.EmailConfirmed = true;
                    user.FullName = Student.FullName;

                    var identityResult = await _userManager.UpdateAsync(user);

                    if (!identityResult.Succeeded)
                    {
                        foreach (var error in identityResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }

                        return Page();
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(Student.Id))
                {
                    return NotFound();
                }

                throw;
            }

            TempData["SuccessMessage"] = "Student updated successfully.";
            return RedirectToPage("./Index");
        }

        private void LoadClassRooms(object? selectedClassRoom = null)
        {
            ViewData["ClassRoomId"] = new SelectList(
                _context.ClassRooms.OrderBy(c => c.Name),
                "Id",
                "Name",
                selectedClassRoom);
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}