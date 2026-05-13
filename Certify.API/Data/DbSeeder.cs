using Certify.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Certify.API.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CertifyDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await context.Database.EnsureCreatedAsync();

        // 1. Seed Roles
        var roles = new[] { "Trainee", "Instructor", "TrainingCoordinator" };
        foreach (var r in roles)
            if (!await roleManager.RoleExistsAsync(r))
                await roleManager.CreateAsync(new IdentityRole(r));

        // 2. Seed Users
        var coord = await CreateUser(userManager, "coord@certify.com", "P@ssw0rd123!", "TrainingCoordinator", "Sarah Manager");
        var instrAhmed = await CreateUser(userManager, "ahmed@certify.com", "P@ssw0rd123!", "Instructor", "Dr. Ahmed Tech");
        var instrLayla = await CreateUser(userManager, "layla@certify.com", "P@ssw0rd123!", "Instructor", "Ms. Layla Design");
        var trainee1 = await CreateUser(userManager, "ali@certify.com", "P@ssw0rd123!", "Trainee", "Ali Student");
        var trainee2 = await CreateUser(userManager, "noor@certify.com", "P@ssw0rd123!", "Trainee", "Noor Learner");

        // 3. Seed Instructors (Business Entity)
        if (!await context.Instructors.AnyAsync())
        {
            context.Instructors.AddRange(
                new Instructor { Name = "Dr. Ahmed Tech", Email = "ahmed@certify.com", ExpertiseAreas = "C#, .NET, Azure" },
                new Instructor { Name = "Ms. Layla Design", Email = "layla@certify.com", ExpertiseAreas = "UI/UX, React, Figma" }
            );
            await context.SaveChangesAsync();
        }
        var dbInstrAhmed = await context.Instructors.FirstAsync(i => i.Email == "ahmed@certify.com");
        var dbInstrLayla = await context.Instructors.FirstAsync(i => i.Email == "layla@certify.com");

        // 4. Seed Rooms
        if (!await context.Rooms.AnyAsync())
        {
            context.Rooms.AddRange(
                new Room { Name = "Lab A", Capacity = 20, Equipment = "30 PCs, Projector" },
                new Room { Name = "Seminar 1", Capacity = 40, Equipment = "Whiteboard, Projector, Mic" },
                new Room { Name = "Workshop B", Capacity = 15, Equipment = "Servers, Lab Equipment" }
            );
            await context.SaveChangesAsync();
        }
        var rooms = await context.Rooms.ToListAsync();

        // 5. Seed Courses & Prerequisites
        if (!await context.Courses.AnyAsync())
        {
            var c1 = new Course { Title = "C# Fundamentals", Category = "Development", DurationHours = 40, Capacity = 20, Fee = 150.00m };
            var c2 = new Course { Title = "ASP.NET Core Web API", Category = "Development", DurationHours = 30, Capacity = 20, Fee = 200.00m };
            var c3 = new Course { Title = "Advanced SQL & EF Core", Category = "Data", DurationHours = 25, Capacity = 20, Fee = 180.00m };
            var c4 = new Course { Title = "UI/UX Design Principles", Category = "Design", DurationHours = 20, Capacity = 25, Fee = 120.00m };
            context.Courses.AddRange(c1, c2, c3, c4);
            await context.SaveChangesAsync();

            // Set prerequisite after initial save to get FK IDs
            var csharp = await context.Courses.FirstAsync(c => c.Title == "C# Fundamentals");
            var aspnet = await context.Courses.FirstAsync(c => c.Title == "ASP.NET Core Web API");
            aspnet.PrerequisiteCourseId = csharp.Id;
            await context.SaveChangesAsync();
        }
        var dbCourses = await context.Courses.ToListAsync();

        // 6. Seed Certification Tracks & Junction
        if (!await context.CertificationTracks.AnyAsync())
        {
            var track = new CertificationTrack { Name = "Full Stack Developer", Description = "Backend + DB + Frontend mastery." };
            context.CertificationTracks.Add(track);
            await context.SaveChangesAsync();

            if (!await context.TrackCourses.AnyAsync())
            {
                context.TrackCourses.AddRange(
                    new TrackCourse { CertificationTrackId = track.Id, CourseId = dbCourses[0].Id },
                    new TrackCourse { CertificationTrackId = track.Id, CourseId = dbCourses[1].Id },
                    new TrackCourse { CertificationTrackId = track.Id, CourseId = dbCourses[2].Id }
                );
                await context.SaveChangesAsync();
            }
        }
        var dbTrack = await context.CertificationTracks.FirstAsync();

        // 7. Seed Scheduled Sessions
        if (!await context.ScheduledSessions.AnyAsync())
        {
            context.ScheduledSessions.AddRange(
                new ScheduledSession { CourseId = dbCourses[0].Id, InstructorId = dbInstrAhmed.Id, RoomId = rooms[0].Id, StartDateTime = new DateTime(2026, 6, 1, 9, 0, 0), EndDateTime = new DateTime(2026, 6, 1, 17, 0, 0), Capacity = 20 },
                new ScheduledSession { CourseId = dbCourses[1].Id, InstructorId = dbInstrAhmed.Id, RoomId = rooms[1].Id, StartDateTime = new DateTime(2026, 6, 15, 9, 0, 0), EndDateTime = new DateTime(2026, 6, 15, 17, 0, 0), Capacity = 18 },
                new ScheduledSession { CourseId = dbCourses[3].Id, InstructorId = dbInstrLayla.Id, RoomId = rooms[2].Id, StartDateTime = new DateTime(2026, 7, 1, 10, 0, 0), EndDateTime = new DateTime(2026, 7, 1, 15, 0, 0), Capacity = 25 }
            );
            await context.SaveChangesAsync();
        }
        var dbSessions = await context.ScheduledSessions.ToListAsync();

        // 8. Seed Enrollments, Payments, Notifications, Certifications
        if (!await context.Enrollments.AnyAsync())
        {
            // Trainee 1 -> C# (Completed/Pass), -> API (Attending/Paid)
            var enr1 = new Enrollment { TraineeId = trainee1.Id, ScheduledSessionId = dbSessions[0].Id, Status = "Completed", PaymentStatus = "Paid", Result = "Pass", EnrollmentFee = 150.00m };
            var enr2 = new Enrollment { TraineeId = trainee1.Id, ScheduledSessionId = dbSessions[1].Id, Status = "Attending", PaymentStatus = "Paid", Result = null, EnrollmentFee = 200.00m };
            // Trainee 2 -> C# (Dropped)
            var enr3 = new Enrollment { TraineeId = trainee2.Id, ScheduledSessionId = dbSessions[0].Id, Status = "Dropped", PaymentStatus = "Pending", Result = null, EnrollmentFee = 150.00m };
            context.Enrollments.AddRange(enr1, enr2, enr3);
            await context.SaveChangesAsync();

            context.Payments.AddRange(
                new Payment { TraineeId = trainee1.Id, Amount = 150.00m, PaymentDate = DateTime.UtcNow.AddDays(-30), Status = "Completed", Description = "C# Fundamentals" },
                new Payment { TraineeId = trainee1.Id, Amount = 200.00m, PaymentDate = DateTime.UtcNow.AddDays(-10), Status = "Completed", Description = "ASP.NET Core Web API" }
            );

            context.Notifications.AddRange(
                new Notification { UserId = trainee1.Id, Title = "Enrollment Confirmed", Message = "You are enrolled in ASP.NET Core Web API.", IsRead = false, CreatedAt = DateTime.UtcNow.AddDays(-10) },
                new Notification { UserId = trainee2.Id, Title = "Course Dropped", Message = "Your enrollment in C# Fundamentals has been dropped.", IsRead = true, CreatedAt = DateTime.UtcNow.AddDays(-5) }
            );

            // Certification for Trainee 1 (Eligible for Full Stack track)
            context.Certifications.Add(new Certification
            {
                TraineeId = trainee1.Id,
                CertificationTrackId = dbTrack.Id,
                CertificateNumber = "CERT-2026-001",
                IssueDate = DateTime.UtcNow,
                Status = "Eligible"
            });

            await context.SaveChangesAsync();
        }
    }

    private static async Task<ApplicationUser> CreateUser(UserManager<ApplicationUser> um, string email, string pwd, string role, string fullName)
    {
        var existing = await um.FindByEmailAsync(email);
        if (existing != null) return existing;
        var user = new ApplicationUser { UserName = email, Email = email, FullName = fullName, EmailConfirmed = true };
        var res = await um.CreateAsync(user, pwd);
        if (res.Succeeded) await um.AddToRoleAsync(user, role);
        return user;
    }
}
