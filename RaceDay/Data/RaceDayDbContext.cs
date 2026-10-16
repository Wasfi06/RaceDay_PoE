using Microsoft.EntityFrameworkCore;
using RaceDay.Models;

namespace RaceDay.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Table names
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Session>().ToTable("Session");
            modelBuilder.Entity<Event>().ToTable("Event");
            modelBuilder.Entity<Category>().ToTable("Category");
            modelBuilder.Entity<Enrollment>().ToTable("Enrollment");
            modelBuilder.Entity<Result>().ToTable("Result");


            // =========================
            // USER
            // =========================

            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<User>()
                .Property(u => u.FirstName)
                .HasColumnType("varchar(50)")
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.LastName)
                .HasColumnType("varchar(50)")
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasColumnType("varchar(150)")
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.PasswordHash)
                .HasColumnType("varchar(500)")
                .HasMaxLength(500)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasColumnType("varchar(20)")
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.Phone)
                .HasColumnType("varchar(30)")
                .HasMaxLength(30);

            modelBuilder.Entity<User>()
                .Property(u => u.ProfileImageUrl)
                .HasColumnType("varchar(500)")
                .HasMaxLength(500);

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasColumnType("datetime2(0)")
                .HasDefaultValueSql("SYSUTCDATETIME()")
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<User>()
                .HasCheckConstraint(
                    "CK_User_Role",
                    "Role IN ('Organizer','Participant')"
                );


            // =========================
            // SESSION
            // =========================

            modelBuilder.Entity<Session>()
                .HasKey(s => s.SessionId);

            modelBuilder.Entity<Session>()
                .Property(s => s.RoleSnapshot)
                .HasColumnType("varchar(20)")
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasColumnType("datetime2(0)")
                .HasDefaultValueSql("SYSUTCDATETIME()")
                .IsRequired();

            modelBuilder.Entity<Session>()
                .Property(s => s.ExpiresAt)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            modelBuilder.Entity<Session>()
                .Property(s => s.RevokedAt)
                .HasColumnType("datetime2(0)");

            modelBuilder.Entity<Session>()
                .HasCheckConstraint(
                    "CK_Session_Role",
                    "RoleSnapshot IN ('Organizer','Participant')"
                );

            modelBuilder.Entity<Session>()
                .HasCheckConstraint(
                    "CK_Session_Expiry",
                    "ExpiresAt > CreatedAt"
                );

            modelBuilder.Entity<Session>()
                .HasOne(s => s.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            // =========================
            // EVENT
            // =========================

            modelBuilder.Entity<Event>()
                .HasKey(e => e.EventId);

            modelBuilder.Entity<Event>()
                .Property(e => e.Name)
                .HasColumnType("varchar(150)")
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<Event>()
                .Property(e => e.Description)
                .HasColumnType("varchar(1000)")
                .HasMaxLength(1000)
                .IsRequired();

            modelBuilder.Entity<Event>()
                .Property(e => e.EventDate)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            modelBuilder.Entity<Event>()
                .Property(e => e.Location)
                .HasColumnType("varchar(200)")
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<Event>()
                .Property(e => e.DistanceKm)
                .HasColumnType("decimal(6,2)")
                .HasPrecision(6, 2)
                .IsRequired();

            modelBuilder.Entity<Event>()
                .Property(e => e.EventType)
                .HasColumnType("varchar(30)")
                .HasMaxLength(30)
                .IsRequired();

            modelBuilder.Entity<Event>()
                .Property(e => e.RouteUrl)
                .HasColumnType("varchar(500)")
                .HasMaxLength(500);

            modelBuilder.Entity<Event>()
                .Property(e => e.RouteDescription)
                .HasColumnType("varchar(1000)")
                .HasMaxLength(1000);

            modelBuilder.Entity<Event>()
                .Property(e => e.BannerImageUrl)
                .HasColumnType("varchar(500)")
                .HasMaxLength(500);

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasColumnType("datetime2(0)")
                .HasDefaultValueSql("SYSUTCDATETIME()")
                .IsRequired();

            modelBuilder.Entity<Event>()
                .Property(e => e.UpdatedAt)
                .HasColumnType("datetime2(0)")
                .HasDefaultValueSql("SYSUTCDATETIME()")
                .IsRequired();

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organizer)
                .WithMany(u => u.OrganizedEvents)
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Event>()
                .HasCheckConstraint(
                    "CK_Event_Distance",
                    "DistanceKm > 0"
                );

            modelBuilder.Entity<Event>()
                .HasCheckConstraint(
                    "CK_Event_Type",
                    "EventType IN ('Running','Walking','Cycling')"
                );


            // =========================
            // CATEGORY
            // =========================

            modelBuilder.Entity<Category>()
                .HasKey(c => c.CategoryId);

            modelBuilder.Entity<Category>()
                .Property(c => c.Name)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Category>()
                .Property(c => c.CategoryType)
                .HasColumnType("varchar(20)")
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<Category>()
                .Property(c => c.MinDistanceKm)
                .HasColumnType("decimal(6,2)")
                .HasPrecision(6, 2);

            modelBuilder.Entity<Category>()
                .Property(c => c.MaxDistanceKm)
                .HasColumnType("decimal(6,2)")
                .HasPrecision(6, 2);

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasColumnType("datetime2(0)")
                .HasDefaultValueSql("SYSUTCDATETIME()")
                .IsRequired();

            modelBuilder.Entity<Category>()
                .HasOne(c => c.Event)
                .WithMany(e => e.Categories)
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Category>()
                .HasIndex(c => new { c.EventId, c.Name })
                .IsUnique();

            modelBuilder.Entity<Category>()
                .HasCheckConstraint(
                    "CK_Category_Type",
                    "CategoryType IN ('Age','Distance')"
                );

            modelBuilder.Entity<Category>()
                .HasCheckConstraint(
                    "CK_Category_Age",
                    "CategoryType <> 'Age' OR " +
                    "(MinAge IS NOT NULL AND MaxAge IS NOT NULL " +
                    "AND MinAge >= 0 AND MaxAge >= MinAge)"
                );

            modelBuilder.Entity<Category>()
                .HasCheckConstraint(
                    "CK_Category_Distance",
                    "CategoryType <> 'Distance' OR " +
                    "(MinDistanceKm IS NOT NULL AND MaxDistanceKm IS NOT NULL " +
                    "AND MinDistanceKm >= 0 AND MaxDistanceKm >= MinDistanceKm)"
                );


            // =========================
            // ENROLLMENT
            // =========================

            modelBuilder.Entity<Enrollment>()
                .HasKey(e => e.EnrollmentId);

            modelBuilder.Entity<Enrollment>()
                .Property(e => e.EnrollmentDate)
                .HasColumnType("datetime2(0)")
                .HasDefaultValueSql("SYSUTCDATETIME()")
                .IsRequired();

            modelBuilder.Entity<Enrollment>()
                .Property(e => e.Status)
                .HasColumnType("varchar(20)")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("Confirmed");

            modelBuilder.Entity<Enrollment>()
                .HasCheckConstraint(
                    "CK_Enrollment_Status",
                    "Status IN ('Pending','Confirmed','Cancelled')"
                );

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Participant)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Event)
                .WithMany(e => e.Enrollments)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasIndex(e => new { e.EventId, e.ParticipantId })
                .IsUnique();


            // =========================
            // RESULT
            // =========================

            modelBuilder.Entity<Result>()
                .HasKey(r => r.ResultId);

            modelBuilder.Entity<Result>()
                .Property(r => r.FinishTime)
                .HasColumnType("time(0)");

            modelBuilder.Entity<Result>()
                .Property(r => r.FinishPosition)
                .IsRequired(false);

            modelBuilder.Entity<Result>()
                .Property(r => r.IsPublished)
                .HasDefaultValue(false);

            modelBuilder.Entity<Result>()
                .Property(r => r.PublishedAt)
                .HasColumnType("datetime2(0)");

            modelBuilder.Entity<Result>()
                .HasOne(r => r.Enrollment)
                .WithOne(e => e.Result)
                .HasForeignKey<Result>(r => r.EnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Result>()
                .HasIndex(r => r.EnrollmentId)
                .IsUnique();

            modelBuilder.Entity<Result>()
                .HasCheckConstraint(
                    "CK_Result_Position",
                    "FinishPosition IS NULL OR FinishPosition > 0"
                );

            modelBuilder.Entity<Result>()
                .HasCheckConstraint(
                    "CK_Result_PublishedAt",
                    "IsPublished = 0 OR PublishedAt IS NOT NULL"
                );
        }
    }
}