using App.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace App.Infrastructure.EFCore;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AttendanceCheckEntity> AttendanceChecks { get; set; }
    public DbSet<AttendanceTypeEntity> AttendanceTypes { get; set; }
    public DbSet<AttendanceEntity> Attendances { get; set; }
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
    
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<DateTime>()
            .HaveConversion<UtcDateTimeConverter>();
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("educode");
        
        // UserEntity relationship
        modelBuilder.Entity<UserEntity>()
            .ToTable("Users")
            .HasQueryFilter(c => c.Deleted == false)
            .HasOne(u => u.Type)
            .WithMany()
            .HasForeignKey(u => u.TypeId);
        modelBuilder.Entity<UserEntity>()
            .HasIndex(u => new {u.Email, u.StudentCode})
            .IsUnique();
        modelBuilder.Entity<UserEntity>()
            .HasIndex(u => u.FullName);
        modelBuilder.Entity<UserEntity>()
            .HasIndex(u => u.Email);
        modelBuilder.Entity<UserEntity>()
            .HasIndex(u => u.StudentCode);
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
        
        // Attendance relationship
        modelBuilder.Entity<AttendanceEntity>()
            .ToTable("Attendances")
            .HasQueryFilter(c => c.Deleted == false);
        modelBuilder.Entity<AttendanceEntity>()
            .HasAlternateKey(c => c.Identifier);
        modelBuilder.Entity<AttendanceEntity>()
            .HasIndex(c => c.Identifier)
            .IsUnique();
        modelBuilder.Entity<AttendanceEntity>()
            .HasOne(c => c.Course)
            .WithMany()
            .HasForeignKey(c => c.CourseId);
        modelBuilder.Entity<AttendanceEntity>()
            .HasOne(c => c.Classroom)
            .WithMany(c => c.Attendances)
            .HasForeignKey(c => c.ClassroomId);
        modelBuilder.Entity<AttendanceEntity>()
            .HasOne(c => c.Type)
            .WithMany()
            .HasForeignKey(c => c.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // AttendanceCheck relationship
        modelBuilder.Entity<AttendanceCheckEntity>()
            .ToTable("AttendanceChecks")
            .HasQueryFilter(c => c.Deleted == false);
         modelBuilder.Entity<AttendanceCheckEntity>()
            .HasIndex(a => new { a.StudentCode, a.AttendanceIdentifier })
            .IsUnique();
         modelBuilder.Entity<AttendanceCheckEntity>()
             .HasIndex(a => a.AttendanceIdentifier);
         modelBuilder.Entity<AttendanceCheckEntity>()
             .HasIndex(a => a.FullName);
         modelBuilder.Entity<AttendanceCheckEntity>()
             .HasIndex(a => a.StudentCode);
         modelBuilder.Entity<AttendanceCheckEntity>()
             .HasOne(a => a.Attendance)
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
            .HasOne(c => c.Status)
            .WithMany()
            .HasForeignKey(c => c.SchoolId);
        modelBuilder.Entity<CourseEntity>()
            .HasMany(c => c.Teachers)
            .WithOne(c => c.Course)
            .HasForeignKey(c => c.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CourseEntity>()
            .HasIndex(c => c.Code)
            .IsUnique();
        modelBuilder.Entity<CourseEntity>()
            .HasIndex(c => c.Name);
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
            .HasIndex(c => c.StatusName)
            .IsUnique();
        
        // CourseTeacher relationship
        modelBuilder.Entity<CourseTeacherEntity>()
            .ToTable("CourseTeachers")
            .HasQueryFilter(c => c.Deleted == false)
            .HasOne(c => c.Course)
            .WithMany(c => c.Teachers)
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
            .HasIndex(u => u.TypeName)
            .IsUnique();
        modelBuilder.Entity<UserTypeEntity>()
            .Property(u => u.AccessLevel)
            .HasConversion<int>();
        
        // AttendanceType relationship
        modelBuilder.Entity<AttendanceTypeEntity>()
            .ToTable("AttendanceTypes")
            .HasQueryFilter(c => c.Deleted == false);
        modelBuilder.Entity<AttendanceTypeEntity>()
            .HasIndex(a => a.TypeName)
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
            .HasIndex(s => s.Name);
        modelBuilder.Entity<SchoolEntity>()
            .HasIndex(s => s.ShortName);
        modelBuilder.Entity<SchoolEntity>()
            .HasIndex(s => s.Domain);
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
            .HasOne(c => c.School)
            .WithMany()
            .HasForeignKey(c => c.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ClassroomEntity>()
            .HasIndex(c => c.Classroom);
    }
}

public class UtcDateTimeConverter() : 
    ValueConverter<DateTime, DateTime>(toDb => toDb.Kind == DateTimeKind.Utc
    ? toDb : DateTime.SpecifyKind(toDb, DateTimeKind.Utc), fromDb => DateTime.SpecifyKind(fromDb, DateTimeKind.Utc));

