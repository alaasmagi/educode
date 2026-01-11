using App.Domain;
using Microsoft.EntityFrameworkCore;
namespace App.DAL.EF;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AttendanceCheckEntity> AttendanceChecks { get; set; }
    public DbSet<AttendanceTypeEntity> AttendanceTypes { get; set; }
    public DbSet<CourseAttendanceEntity> CourseAttendances { get; set; }
    public DbSet<CourseEntity> Courses { get; set; }
    public DbSet<CourseStatusEntity> CourseStatuses { get; set; }
    public DbSet<CourseTeacherEntity> CourseTeachers { get; set; }
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<UserTypeEntity> UserTypes { get; set; }
    public DbSet<WorkplaceEntity> Workplaces { get; set; }
    public DbSet<UserAuthEntity> UserAuthData { get; set; }
    public DbSet<SchoolEntity> Schools { get; set; }
    public DbSet<ClassroomEntity> Classrooms { get; set; }
    public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("educode");
        
        // UserEntity relationship
        modelBuilder.Entity<UserEntity>()
            .ToTable("Users")
            .HasQueryFilter(c => c.Deleted == false)
            .HasOne(u => u.UserType)
            .WithMany()
            .HasForeignKey(u => u.UserTypeId);
        modelBuilder.Entity<UserEntity>()
            .HasIndex(u => new {u.Email, u.StudentCode})
            .IsUnique();
        modelBuilder.Entity<UserEntity>()
            .HasIndex(u => u.FullName);
        modelBuilder.Entity<UserEntity>()
            .HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<UserEntity>()
            .HasOne(u => u.School)
            .WithMany()
            .HasForeignKey(u => u.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // UserAuth relationship
        modelBuilder.Entity<UserAuthEntity>()
            .ToTable("UserAuthData")
            .HasQueryFilter(c => c.Deleted == false)
            .HasOne(u => u.User)
            .WithOne()
            .HasForeignKey<UserAuthEntity>(u => u.UserId);
        modelBuilder.Entity<UserAuthEntity>()
            .HasIndex(u => u.UserId)
            .IsUnique();
        
        // CourseAttendance relationship
        modelBuilder.Entity<CourseAttendanceEntity>()
            .ToTable("CourseAttendances")
            .HasQueryFilter(c => c.Deleted == false);
        modelBuilder.Entity<CourseAttendanceEntity>()
            .HasAlternateKey(c => c.Identifier);
        modelBuilder.Entity<CourseAttendanceEntity>()
            .HasIndex(c => c.Identifier)
            .IsUnique();
        modelBuilder.Entity<CourseAttendanceEntity>()
            .HasOne(c => c.Course)
            .WithMany()
            .HasForeignKey(c => c.CourseId);
        modelBuilder.Entity<CourseAttendanceEntity>()
            .HasOne(c => c.Classroom)
            .WithMany(c => c.CourseAttendances)
            .HasForeignKey(c => c.ClassroomId);
        modelBuilder.Entity<CourseAttendanceEntity>()
            .HasOne(c => c.AttendanceType)
            .WithMany()
            .HasForeignKey(c => c.AttendanceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // AttendanceCheck relationship
        modelBuilder.Entity<AttendanceCheckEntity>()
            .ToTable("AttendanceChecks")
            .HasQueryFilter(c => c.Deleted == false);
         modelBuilder.Entity<AttendanceCheckEntity>()
            .HasIndex(a => new { a.StudentCode, a.AttendanceIdentifier })
            .IsUnique();
         modelBuilder.Entity<AttendanceCheckEntity>()
             .HasOne(a => a.CourseAttendance)
             .WithMany(c => c.AttendanceChecks)
             .HasForeignKey(a => a.AttendanceIdentifier)
             .HasPrincipalKey(c => c.Identifier)
             .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<AttendanceCheckEntity>()
            .HasOne(a => a.Workplace)
            .WithMany()
            .HasForeignKey(a => a.WorkplaceIdentifier)
            .HasPrincipalKey(w => w.Identifier);
        
        // Course relationship
        modelBuilder.Entity<CourseEntity>()
            .ToTable("Courses")
            .HasQueryFilter(c => c.Deleted == false)
            .HasOne(c => c.CourseStatus)
            .WithMany()
            .HasForeignKey(c => c.CourseStatusId);
        modelBuilder.Entity<CourseEntity>()
            .HasMany(c => c.CourseTeacherEntities)
            .WithOne(c => c.Course)
            .HasForeignKey(c => c.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CourseEntity>()
            .HasIndex(c => c.CourseCode)
            .IsUnique();
        modelBuilder.Entity<CourseEntity>()
            .HasOne(c => c.School)
            .WithMany()
            .HasForeignKey(c => c.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // CourseStatus relationship
        modelBuilder.Entity<CourseStatusEntity>()
            .ToTable("CourseStatuses")            
            .HasQueryFilter(c => c.Deleted == false);
        modelBuilder.Entity<CourseStatusEntity>()
            .HasIndex(c => c.CourseStatus)
            .IsUnique();
        
        // CourseTeacher relationship
        modelBuilder.Entity<CourseTeacherEntity>()
            .ToTable("CourseTeachers")
            .HasQueryFilter(c => c.Deleted == false)
            .HasOne(c => c.Course)
            .WithMany(c => c.CourseTeacherEntities)
            .HasForeignKey(c => c.CourseId);
        modelBuilder.Entity<CourseTeacherEntity>()
            .HasOne(c => c.Teacher)
            .WithMany()
            .HasForeignKey(c => c.TeacherId);
        
        // UserType relationship
        modelBuilder.Entity<UserTypeEntity>()
            .ToTable("UserTypes")
            .HasQueryFilter(c => c.Deleted == false);
        modelBuilder.Entity<UserTypeEntity>()
            .HasIndex(u => u.UserType)
            .IsUnique();
        modelBuilder.Entity<UserTypeEntity>()
            .Property(u => u.AccessLevel)
            .HasConversion<int>();
        
        // AttendanceType relationship
        modelBuilder.Entity<AttendanceTypeEntity>()
            .ToTable("AttendanceTypes")
            .HasQueryFilter(c => c.Deleted == false);
        modelBuilder.Entity<AttendanceTypeEntity>()
            .HasIndex(a => a.AttendanceType)
            .IsUnique();
        
        // Workplace relationship
        modelBuilder.Entity<WorkplaceEntity>()
            .ToTable("Workplaces")
            .HasQueryFilter(c => c.Deleted == false);
        modelBuilder.Entity<WorkplaceEntity>()
            .HasIndex(w => w.Identifier)
            .IsUnique();
        modelBuilder.Entity<WorkplaceEntity>()
            .HasOne(w => w.Classroom)
            .WithMany()
            .HasForeignKey(w => w.ClassroomId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<WorkplaceEntity>()
            .HasAlternateKey(w => w.Identifier);
        
        // School relationship
        modelBuilder.Entity<SchoolEntity>()
            .ToTable("Schools")
            .HasQueryFilter(r => r.Deleted == false);
        modelBuilder.Entity<SchoolEntity>()
            .HasIndex(s => new {s.Name, s.ShortName, s.Domain})
            .IsUnique();
        modelBuilder.Entity<SchoolEntity>()
            .HasMany(s => s.Classrooms)
            .WithOne(s => s.School)
            .HasForeignKey(s => s.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // RefreshToken relationship
        modelBuilder.Entity<RefreshTokenEntity>()
            .ToTable("RefreshTokens")
            .HasQueryFilter(r => r.Deleted == false);
        modelBuilder.Entity<RefreshTokenEntity>()
            .HasIndex(r => r.Token)
            .IsUnique();
        modelBuilder.Entity<RefreshTokenEntity>()
            .HasIndex(r => r.UserId);

        // ClassroomEntity relationship
        modelBuilder.Entity<ClassroomEntity>()
            .ToTable("Classrooms")
            .HasQueryFilter(c => c.Deleted == false);
        modelBuilder.Entity<ClassroomEntity>()
            .HasOne(w => w.School)
            .WithMany()
            .HasForeignKey(w => w.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
