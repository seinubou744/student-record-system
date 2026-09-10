using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Data;
using StudentRecordSystem.Models;
using StudentRecordSystem.Services;

namespace StudentRecordSystem.Controllers.Api
{
    [ApiController]
    [Route("api/timetable")]
    public class TimetableController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TimetableService _timetableService;

        public TimetableController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, TimetableService timetableService)
        {
            _context = context;
            _userManager = userManager;
            _timetableService = timetableService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TimetableEntry model)
        {
            var error = await _timetableService.ValidateEntryAsync(model);
            if (error is not null) return BadRequest(new { message = error });

            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;
            _context.TimetableEntries.Add(model);
            await _context.SaveChangesAsync();
            return Ok(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] TimetableEntry model)
        {
            var existing = await _context.TimetableEntries.FindAsync(id);
            if (existing is null) return NotFound();

            existing.ClassRoomId = model.ClassRoomId;
            existing.SectionId = model.SectionId;
            existing.SubjectId = model.SubjectId;
            existing.TeacherId = model.TeacherId;
            existing.DayOfWeek = model.DayOfWeek;
            existing.StartTime = model.StartTime;
            existing.EndTime = model.EndTime;
            existing.RoomNumber = model.RoomNumber;
            existing.AcademicYear = model.AcademicYear;
            existing.UpdatedAt = DateTime.UtcNow;

            var error = await _timetableService.ValidateEntryAsync(existing, id);
            if (error is not null) return BadRequest(new { message = error });

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _context.TimetableEntries.FindAsync(id);
            if (existing is null) return NotFound();

            _context.TimetableEntries.Remove(existing);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize]
        [HttpGet("class/{classId:int}/section/{sectionId:int}")]
        public async Task<IActionResult> GetClassTimetable(int classId, int sectionId, [FromQuery] string? academicYear)
        {
            var query = _context.TimetableEntries
                .Include(t => t.Subject)
                .Include(t => t.Teacher)
                .Where(t => t.ClassRoomId == classId && t.SectionId == sectionId);

            if (!string.IsNullOrWhiteSpace(academicYear))
                query = query.Where(t => t.AcademicYear == academicYear);

            var result = await query.OrderBy(t => t.DayOfWeek).ThenBy(t => t.StartTime).ToListAsync();
            return Ok(result);
        }

        [Authorize(Roles = "Teacher")]
        [HttpGet("teacher")]
        public async Task<IActionResult> GetTeacherTimetable([FromQuery] string? academicYear)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Unauthorized();

            var query = _context.TimetableEntries
                .Include(t => t.Subject)
                .Include(t => t.ClassRoom)
                .Include(t => t.Section)
                .Where(t => t.TeacherId == user.Id);

            if (!string.IsNullOrWhiteSpace(academicYear))
                query = query.Where(t => t.AcademicYear == academicYear);

            var result = await query.OrderBy(t => t.DayOfWeek).ThenBy(t => t.StartTime).ToListAsync();
            return Ok(result);
        }

        [Authorize(Roles = "Student")]
        [HttpGet("student")]
        public async Task<IActionResult> GetStudentTimetable([FromQuery] string? academicYear)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Unauthorized();

            var student = await _context.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id);
            if (student is null) return NotFound(new { message = "Student record not found." });

            var query = _context.TimetableEntries
                .Include(t => t.Subject)
                .Include(t => t.Teacher)
                .Where(t => t.ClassRoomId == student.ClassRoomId && t.SectionId == student.SectionId);

            if (!string.IsNullOrWhiteSpace(academicYear))
                query = query.Where(t => t.AcademicYear == academicYear);

            var result = await query.OrderBy(t => t.DayOfWeek).ThenBy(t => t.StartTime).ToListAsync();
            return Ok(result);
        }
    }
}