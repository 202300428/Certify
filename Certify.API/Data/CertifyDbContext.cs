using Certify.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Certify.API.Data;

public class CertifyDbContext(DbContextOptions<CertifyDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Course> Courses { get; set; }
    public DbSet<Instructor> Instructors { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<ScheduledSession> ScheduledSessions { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<Certification> Certifications { get; set; }
    public DbSet<CertificationTrack> CertificationTracks { get; set; }
    public DbSet<TrackCourse> TrackCourses { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .HasMany<Notification>()
            .WithOne(n => n.User)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Course>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Title).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Description).HasMaxLength(1000);
            entity.Property(c => c.Category).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Fee).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(c => c.Capacity).IsRequired();
            entity.Property(c => c.DurationHours).IsRequired();

            // Self-referencing prerequisite (nullable, restrict delete)
            entity.HasOne(c => c.PrerequisiteCourse)
                .WithMany()
                .HasForeignKey(c => c.PrerequisiteCourseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(c => c.Category);
        });

        builder.Entity<Instructor>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Name).IsRequired().HasMaxLength(200);
            entity.Property(i => i.Email).IsRequired().HasMaxLength(255);
            entity.Property(i => i.ExpertiseAreas).HasMaxLength(500);
            entity.HasIndex(i => i.Email).IsUnique();
        });

        builder.Entity<Room>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(100);
            entity.Property(r => r.Capacity).IsRequired();
            entity.Property(r => r.Equipment).HasMaxLength(500);
        });

        builder.Entity<CertificationTrack>(entity =>
        {
            entity.HasKey(ct => ct.Id);
            entity.Property(ct => ct.Name).IsRequired().HasMaxLength(200);
            entity.Property(ct => ct.Description).HasMaxLength(1000);
        });

        builder.Entity<TrackCourse>()
            .HasKey(tc => new { tc.CertificationTrackId, tc.CourseId });

        builder.Entity<TrackCourse>()
            .HasOne(tc => tc.CertificationTrack)
            .WithMany(ct => ct.TrackCourses)
            .HasForeignKey(tc => tc.CertificationTrackId);

        builder.Entity<TrackCourse>()
            .HasOne(tc => tc.Course)
            .WithMany()
            .HasForeignKey(tc => tc.CourseId);

        builder.Entity<ScheduledSession>(entity =>
        {
            entity.HasKey(ss => ss.Id);
            entity.Property(ss => ss.StartDateTime).IsRequired();
            entity.Property(ss => ss.EndDateTime).IsRequired();
            entity.Property(ss => ss.Capacity).IsRequired();

            entity.HasCheckConstraint("CK_Session_DateRange", "[EndDateTime] > [StartDateTime]");
            entity.HasCheckConstraint("CK_Session_Capacity", "[Capacity] > 0");

            entity.HasIndex(ss => new { ss.InstructorId, ss.StartDateTime, ss.EndDateTime });
            entity.HasIndex(ss => new { ss.RoomId, ss.StartDateTime, ss.EndDateTime });

            entity.HasOne(ss => ss.Course)
                .WithMany(c => c.ScheduledSessions)
                .HasForeignKey(ss => ss.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ss => ss.Instructor)
                .WithMany(i => i.ScheduledSessions)
                .HasForeignKey(ss => ss.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ss => ss.Room)
                .WithMany(r => r.ScheduledSessions)
                .HasForeignKey(ss => ss.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => new { e.TraineeId, e.ScheduledSessionId }).IsUnique();

            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.PaymentStatus).HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.Result).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.EnrollmentFee).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Trainee)
                .WithMany(t => t.Enrollments)
                .HasForeignKey(e => e.TraineeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ScheduledSession)
                .WithMany(ss => ss.Enrollments)
                .HasForeignKey(e => e.ScheduledSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Amount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(p => p.PaymentDate).IsRequired();
            entity.Property(p => p.Status).HasConversion<string>().HasMaxLength(50);

            entity.HasOne(p => p.Trainee)
                .WithMany(t => t.Payments)
                .HasForeignKey(p => p.TraineeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Certification>(entity =>
        {
            entity.HasKey(c => c.Id);

            // One certification per trainee per track
            entity.HasIndex(c => new { c.TraineeId, c.CertificationTrackId }).IsUnique();

            entity.Property(c => c.CertificateNumber).HasMaxLength(50);
            entity.Property(c => c.IssueDate).IsRequired();
            entity.Property(c => c.Status).HasConversion<string>().HasMaxLength(50);

            entity.HasOne(c => c.Trainee)
                .WithMany(t => t.Certifications)
                .HasForeignKey(c => c.TraineeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.CertificationTrack)
                .WithMany(ct => ct.Certifications)
                .HasForeignKey(c => c.CertificationTrackId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Message).IsRequired().HasMaxLength(500);
            entity.Property(n => n.CreatedAt).IsRequired();
            entity.Property(n => n.IsRead).HasDefaultValue(false);
        });

        builder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = "1", Name = "Trainee", NormalizedName = "TRAINEE" },
            new IdentityRole { Id = "2", Name = "Instructor", NormalizedName = "INSTRUCTOR" },
            new IdentityRole { Id = "3", Name = "TrainingCoordinator", NormalizedName = "TRAININGCOORDINATOR" }
        );
    }
}
