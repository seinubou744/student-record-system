using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentRecordSystem.Models;

namespace StudentRecordSystem.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();


            try
            {
                await context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Migration skipped: {ex.Message}");
            }

            string[] roles = { "Admin", "Student", "Teacher" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(role));

                    if (!roleResult.Succeeded)
                    {
                        throw new Exception(string.Join("; ", roleResult.Errors.Select(e => e.Description)));
                    }
                }
            }

            var adminEmail = "admin@studentrecordsystem.com";
            var adminPassword = "Admin123!";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true
                };

                var createAdminResult = await userManager.CreateAsync(adminUser, adminPassword);

                if (!createAdminResult.Succeeded)
                {
                    throw new Exception(string.Join("; ", createAdminResult.Errors.Select(e => e.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                var addToRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");

                if (!addToRoleResult.Succeeded)
                {
                    throw new Exception(string.Join("; ", addToRoleResult.Errors.Select(e => e.Description)));
                }
            }

            if (!context.Sections.Any())
            {
                var firstClass = await context.ClassRooms.OrderBy(c => c.Id).FirstOrDefaultAsync();
                if (firstClass != null)
                {
                    context.Sections.AddRange(
                        new Section { ClassRoomId = firstClass.Id, Name = "A", Description = "Morning Section" },
                        new Section { ClassRoomId = firstClass.Id, Name = "B", Description = "Afternoon Section" }
                    );
                    await context.SaveChangesAsync();
                }
            }

            if (!context.TimetableEntries.Any())
            {
                var firstClass = await context.ClassRooms.FirstOrDefaultAsync();
                var firstSection = await context.Sections.FirstOrDefaultAsync();
                var firstSubject = await context.Subjects.FirstOrDefaultAsync();
                var firstTeacher = await userManager.GetUsersInRoleAsync("Teacher");

                if (firstClass != null && firstSection != null && firstSubject != null && firstTeacher.Any())
                {
                    context.TimetableEntries.AddRange(
                        new TimetableEntry
                        {
                            ClassRoomId = firstClass.Id,
                            SectionId = firstSection.Id,
                            SubjectId = firstSubject.Id,
                            TeacherId = firstTeacher.First().Id,
                            DayOfWeek = WeekDay.Sunday,
                            StartTime = new TimeSpan(8, 0, 0),
                            EndTime = new TimeSpan(9, 0, 0),
                            RoomNumber = "R-101",
                            AcademicYear = "2026-2027",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new TimetableEntry
                        {
                            ClassRoomId = firstClass.Id,
                            SectionId = firstSection.Id,
                            SubjectId = firstSubject.Id,
                            TeacherId = firstTeacher.First().Id,
                            DayOfWeek = WeekDay.Monday,
                            StartTime = new TimeSpan(9, 0, 0),
                            EndTime = new TimeSpan(10, 0, 0),
                            RoomNumber = "R-102",
                            AcademicYear = "2026-2027",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    );

                    await context.SaveChangesAsync();
                }
            }
        }
    }
}