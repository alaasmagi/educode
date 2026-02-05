using App.Domain.Entities;
using Base.Domain;
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
        
        // Configure BaseEntity properties for all entities
        ConfigureBaseEntity<UserEntity>(modelBuilder);
        ConfigureBaseEntity<UserAuthEntity>(modelBuilder);
        ConfigureBaseEntity<UserTypeEntity>(modelBuilder);
        ConfigureBaseEntity<AttendanceEntity>(modelBuilder);
        ConfigureBaseEntity<AttendanceCheckEntity>(modelBuilder);
        ConfigureBaseEntity<AttendanceTypeEntity>(modelBuilder);
        ConfigureBaseEntity<CourseEntity>(modelBuilder);
        ConfigureBaseEntity<CourseStatusEntity>(modelBuilder);
        ConfigureBaseEntity<CourseTeacherEntity>(modelBuilder);
        ConfigureBaseEntity<WorkplaceEntity>(modelBuilder);
        ConfigureBaseEntity<SchoolEntity>(modelBuilder);
        ConfigureBaseEntity<ClassroomEntity>(modelBuilder);
        ConfigureBaseEntity<RefreshTokenEntity>(modelBuilder);
        
        // UserEntity configuration
        modelBuilder.Entity<UserEntity>()
            .Property(u => u.Email).IsRequired().HasMaxLength(128);
        modelBuilder.Entity<UserEntity>()
            .Property(u => u.StudentCode).HasMaxLength(128);
        modelBuilder.Entity<UserEntity>()
            .Property(u => u.FullName).IsRequired().HasMaxLength(255);
        modelBuilder.Entity<UserEntity>()
            .Property(u => u.PhotoPath).HasMaxLength(255);
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
            .OnDelete(DeleteBehavior.Restrict);
        
        // UserAuth configuration
        modelBuilder.Entity<UserAuthEntity>()
            .Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
        modelBuilder.Entity<UserAuthEntity>()
            .ToTable("UserAuthData")
            .HasQueryFilter(c => c.Deleted == false)
            .HasOne(u => u.User)
            .WithOne()
            .HasForeignKey<UserAuthEntity>(u => u.UserId);
        modelBuilder.Entity<UserAuthEntity>()
            .HasIndex(u => u.UserId)
            .IsUnique();
        
        // Attendance configuration
        modelBuilder.Entity<AttendanceEntity>()
            .Property(a => a.Identifier).IsRequired();
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
            .HasForeignKey(c => c.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<AttendanceEntity>()
            .HasOne(c => c.Classroom)
            .WithMany(c => c.Attendances)
            .HasForeignKey(c => c.ClassroomId);
        modelBuilder.Entity<AttendanceEntity>()
            .HasOne(c => c.Type)
            .WithMany()
            .HasForeignKey(c => c.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // AttendanceCheck configuration
        modelBuilder.Entity<AttendanceCheckEntity>()
            .Property(a => a.StudentCode).IsRequired();
        modelBuilder.Entity<AttendanceCheckEntity>()
            .Property(a => a.FullName).IsRequired().HasMaxLength(255);
        modelBuilder.Entity<AttendanceCheckEntity>()
            .Property(a => a.AttendanceIdentifier).IsRequired();
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
        
        // Course configuration
        modelBuilder.Entity<CourseEntity>()
            .Property(c => c.Code).IsRequired().HasMaxLength(128);
        modelBuilder.Entity<CourseEntity>()
            .Property(c => c.Name).IsRequired().HasMaxLength(128);
        modelBuilder.Entity<CourseEntity>()
            .ToTable("Courses")
            .HasQueryFilter(c => c.Deleted == false)
            .HasOne(c => c.Status)
            .WithMany()
            .HasForeignKey(c => c.StatusId);
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
        
        // CourseStatus configuration
        modelBuilder.Entity<CourseStatusEntity>()
            .Property(c => c.StatusName).IsRequired().HasMaxLength(128);
        modelBuilder.Entity<CourseStatusEntity>()
            .ToTable("CourseStatuses")            
            .HasQueryFilter(c => c.Deleted == false);
        modelBuilder.Entity<CourseStatusEntity>()
            .HasIndex(c => c.StatusName)
            .IsUnique();
        
        // CourseTeacher configuration
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
        
        // UserType configuration
        modelBuilder.Entity<UserTypeEntity>()
            .Property(u => u.TypeName).IsRequired().HasMaxLength(128);
        modelBuilder.Entity<UserTypeEntity>()
            .ToTable("UserTypes")
            .HasQueryFilter(c => c.Deleted == false);
        modelBuilder.Entity<UserTypeEntity>()
            .HasIndex(u => u.TypeName)
            .IsUnique();
        modelBuilder.Entity<UserTypeEntity>()
            .Property(u => u.AccessLevel)
            .HasConversion<int>();
        
        // AttendanceType configuration
        modelBuilder.Entity<AttendanceTypeEntity>()
            .Property(a => a.TypeName).IsRequired().HasMaxLength(128);
        modelBuilder.Entity<AttendanceTypeEntity>()
            .ToTable("AttendanceTypes")
            .HasQueryFilter(c => c.Deleted == false);
        modelBuilder.Entity<AttendanceTypeEntity>()
            .HasIndex(a => a.TypeName)
            .IsUnique();
        
        // Workplace configuration
        modelBuilder.Entity<WorkplaceEntity>()
            .Property(w => w.Identifier).IsRequired();
        modelBuilder.Entity<WorkplaceEntity>()
            .Property(w => w.ComputerCode).IsRequired().HasMaxLength(128);
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
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<WorkplaceEntity>()
            .HasAlternateKey(w => w.Identifier);
        
        // School configuration
        modelBuilder.Entity<SchoolEntity>()
            .Property(s => s.Name).IsRequired().HasMaxLength(255);
        modelBuilder.Entity<SchoolEntity>()
            .Property(s => s.ShortName).IsRequired().HasMaxLength(128);
        modelBuilder.Entity<SchoolEntity>()
            .Property(s => s.Domain).IsRequired().HasMaxLength(255);
        modelBuilder.Entity<SchoolEntity>()
            .Property(s => s.StudentCodePattern).IsRequired().HasMaxLength(128);
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
            .OnDelete(DeleteBehavior.Cascade);
        
        // RefreshToken configuration
        modelBuilder.Entity<RefreshTokenEntity>()
            .Property(r => r.Token).IsRequired().HasMaxLength(256);
        modelBuilder.Entity<RefreshTokenEntity>()
            .Property(r => r.PushNotificationToken).HasMaxLength(256);
        modelBuilder.Entity<RefreshTokenEntity>()
            .Property(r => r.Client).IsRequired().HasMaxLength(128);
        modelBuilder.Entity<RefreshTokenEntity>()
            .Property(r => r.ClientIp).IsRequired().HasMaxLength(128);
        modelBuilder.Entity<RefreshTokenEntity>()
            .ToTable("RefreshTokens")
            .HasQueryFilter(r => r.Deleted == false);
        modelBuilder.Entity<RefreshTokenEntity>()
            .HasIndex(r => r.Token)
            .IsUnique();
        modelBuilder.Entity<RefreshTokenEntity>()
            .HasIndex(r => r.UserId);

        // Classroom configuration
        modelBuilder.Entity<ClassroomEntity>()
            .Property(c => c.Classroom).IsRequired().HasMaxLength(128);
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
    
    private static void ConfigureBaseEntity<TEntity>(ModelBuilder modelBuilder) where TEntity : BaseEntity
    {
        modelBuilder.Entity<TEntity>()
            .HasKey(e => e.Id);
        modelBuilder.Entity<TEntity>()
            .Property(e => e.CreatedBy).IsRequired().HasMaxLength(128);
        modelBuilder.Entity<TEntity>()
            .Property(e => e.CreatedAt).IsRequired();
        modelBuilder.Entity<TEntity>()
            .Property(e => e.UpdatedBy).IsRequired().HasMaxLength(128);
        modelBuilder.Entity<TEntity>()
            .Property(e => e.UpdatedAt).IsRequired();
        modelBuilder.Entity<TEntity>()
            .Property(e => e.Deleted).IsRequired();
    }
}

public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter() : base(
        // Convert to database: ensure UTC
        toDb => toDb.Kind == DateTimeKind.Utc 
            ? toDb 
            : DateTime.SpecifyKind(toDb, DateTimeKind.Utc),
        // Convert from database: ensure UTC
        fromDb => DateTime.SpecifyKind(fromDb, DateTimeKind.Utc))
    {
    }
}

