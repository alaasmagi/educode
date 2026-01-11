using App.BLL;
using App.DAL.EF;
using App.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.BLL_Tests;

[TestFixture]
public class CourseManagementServiceTests
{
    private AppDbContext _context = null!;
    private CourseManagementService _service = null!;
    private ILogger<CourseManagementService> _logger = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        _context = new AppDbContext(options);
        _logger = new LoggerFactory().CreateLogger<CourseManagementService>();
        _service = new CourseManagementService(_context, _logger);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
    
    [Test]
    public async Task GetCourseByAttendanceIdAsync_ReturnsCourse_IfExists()
    {
        var attendanceType = new AttendanceTypeEntity()
        {
            Id = 1,
            AttendanceType = "TEST",
            CreatedBy = "test",
            UpdatedBy = "test",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        var course = new CourseEntity
        {
            Id = 1, 
            CourseName = "Math", 
            CourseCode = "test", 
            CreatedBy = "test", 
            UpdatedBy = "test"
        };
        var attendance = new CourseAttendanceEntity
        {
            Id = 10, 
            AttendanceTypeId = 1,
            AttendanceType = attendanceType,
            CourseId = 1, 
            Course = course,
            CreatedBy = "test", 
            UpdatedBy = "test"
        };
        
        await _context.AttendanceTypes.AddAsync(attendanceType);
        await _context.Courses.AddAsync(course);
        await _context.CourseAttendances.AddAsync(attendance);
        await _context.SaveChangesAsync();
        
        var result = await _service.GetCourseByAttendanceIdAsync(10);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(course.Id));
    }

    [Test]
    public async Task GetCourseByAttendanceIdAsync_ReturnsNull_IfAttendanceNotFound()
    {
        var result = await _service.GetCourseByAttendanceIdAsync(999);
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task GetCourseByIdAsync_ReturnsCourse_IfAccessible()
    {
        var userType = new UserTypeEntity() { 
            Id = 1, 
            UserType = "authTest",
            CreatedBy = "authTest",
            UpdatedBy = "authTest"
        };
        var user = new UserEntity 
        { 
            Id = 1,
            UniId = "exist123", 
            FullName = "test", 
            CreatedBy = "test",
            UpdatedBy = "test", 
            UserTypeId = 1
        };
        var course = new CourseEntity
        {
            Id = 1, 
            CourseName = "Math", 
            CourseCode = "test", 
            CreatedBy = "test", 
            UpdatedBy = "test"
        };
        var courseTeacher = new CourseTeacherEntity
        {
            CourseId = 1, 
            TeacherId = 1,
            CreatedBy = "test", 
            UpdatedBy = "test"
        };
    
        await _context.UserTypes.AddAsync(userType);
        await _context.Users.AddAsync(user);
        await _context.Courses.AddAsync(course);
        await _context.CourseTeachers.AddAsync(courseTeacher);
        await _context.SaveChangesAsync();

        var result = await _service.GetCourseByIdAsync(1, "exist123");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(course.Id));
    }

    [Test]
    public async Task GetCourseByIdAsync_ReturnsNull_IfNotAccessible()
    {
        var user = new UserEntity 
        { 
            Id = 1,
            UniId = "exist123", 
            FullName = "test", 
            CreatedBy = "test",
            UpdatedBy = "test", 
            UserTypeId = 1
        };
        var course = new CourseEntity
        {
            Id = 1, 
            CourseName = "Math", 
            CourseCode = "test", 
            CreatedBy = "test", 
            UpdatedBy = "test"
        };

        await _context.Users.AddAsync(user);
        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();

        var result = await _service.GetCourseByIdAsync(2, "u2");

        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task AddCourse_AddsCourse_IfNotExists()
    {
        var user = new UserEntity 
        { 
            Id = 1,
            UniId = "exist123", 
            FullName = "test", 
            CreatedBy = "test",
            UpdatedBy = "test", 
            UserTypeId = 1
        };
        var course = new CourseEntity
        {
            Id = 1, 
            CourseName = "Math", 
            CourseCode = "test", 
            CreatedBy = "test", 
            UpdatedBy = "test"
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _service.AddCourse(user, course, "creator");

        Assert.That(result, Is.True);
        var createdCourse = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == "test");
        Assert.That(createdCourse, Is.Not.Null);
    }

    [Test]
    public async Task AddCourse_Fails_IfCourseExists()
    {
        var course = new CourseEntity
        {
            Id = 1, 
            CourseName = "Math", 
            CourseCode = "test", 
            CreatedBy = "test", 
            UpdatedBy = "test"
        };
        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();

        var result = await _service.AddCourse(new UserEntity(), course, "creator");

        Assert.That(result, Is.False);
    }
    
    [Test]
    public async Task EditCourse_UpdatesCourse_IfExists()
    {
        var course = new CourseEntity
        {
            Id = 2, 
            CourseName = "Math", 
            CourseCode = "test", 
            CreatedBy = "test", 
            UpdatedBy = "test"
        };
        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();

        var updatedCourse = new CourseEntity
        {
            CourseName = "Updated", 
            CourseCode = "test", 
            CreatedBy = "test", 
            UpdatedBy = "test"
        };
        var result = await _service.EditCourse(2, updatedCourse);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task EditCourse_ReturnsFalse_IfCourseDoesNotExist()
    {
        var result = await _service.EditCourse(100, new CourseEntity());
        Assert.That(result, Is.False);
    }
    
    [Test]
    public async Task DeleteCourse_Deletes_IfAccessible()
    {
        var userType = new UserTypeEntity() { 
            Id = 1, 
            UserType = "authTest",
            CreatedBy = "authTest",
            UpdatedBy = "authTest"
        };
        var user = new UserEntity 
        { 
            Id = 1,
            UniId = "exist123", 
            FullName = "test", 
            CreatedBy = "test",
            UpdatedBy = "test", 
            UserTypeId = 1
        };
        var course = new CourseEntity
        {
            Id = 2, 
            CourseName = "Math", 
            CourseCode = "test", 
            CreatedBy = "test", 
            UpdatedBy = "test"
        };
        var courseTeacher = new CourseTeacherEntity
        {
            CourseId = 2, 
            TeacherId = 1,
            CreatedBy = "test", 
            UpdatedBy = "test"
        };

        await _context.UserTypes.AddAsync(userType);
        await _context.Users.AddAsync(user);
        await _context.Courses.AddAsync(course);
        await _context.CourseTeachers.AddAsync(courseTeacher);
        await _context.SaveChangesAsync();
        
        var result = await _service.DeleteCourse(2, "exist123");

        Assert.That(result, Is.True);
    }
    
    [Test]
    public void GetAllCourseStatuses_ReturnsStatuses()
    {
        var result = _service.GetAllCourseStatuses();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Count, Is.GreaterThan(0));
    }
    
    [Test]
    public async Task DoesCourseExistAsync_ReturnsTrue_IfExists()
    {
        var course = new CourseEntity
        {
            Id = 2, 
            CourseName = "Math", 
            CourseCode = "test", 
            CreatedBy = "test", 
            UpdatedBy = "test"
        };
        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();

        var result = await _context.Courses.AnyAsync(c => c.Id == 2);

        Assert.That(result, Is.True);
    }
    [Test]
    public async Task GetCoursesByUserAsync_ReturnsCourses()
    {
        var course = new CourseEntity
        {
            Id = 2, 
            CourseName = "Math", 
            CourseCode = "test", 
            CreatedBy = "test", 
            UpdatedBy = "test"
        };
        var courseTeacher = new CourseTeacherEntity
        {
            CourseId = 2, 
            TeacherId = 1,
            CreatedBy = "test", 
            UpdatedBy = "test"
        };

        await _context.Courses.AddAsync(course);
        await _context.CourseTeachers.AddAsync(courseTeacher);
        await _context.SaveChangesAsync();

        var result = await _service.GetCoursesByUserAsync(1);

        Assert.That(result, Is.Not.Null);
    }
    
    [Test]
    public async Task GetAttendancesUserCountsByCourseAsync_ReturnsCounts()
    {
        var course = new CourseEntity
        {
            Id = 2, 
            CourseName = "Math", 
            CourseCode = "test", 
            CreatedBy = "test", 
            UpdatedBy = "test"
        };
        var courseAttendance = new CourseAttendanceEntity
        {
            Id = 2, 
            CourseId = 2,
            CreatedBy = "test", 
            UpdatedBy = "test"
        };
        await _context.Courses.AddAsync(course);
        await _context.CourseAttendances.AddAsync(courseAttendance);
        await _context.SaveChangesAsync();

        var result = await _service.GetAttendancesUserCountsByCourseAsync(2);

        Assert.That(result, Is.Not.Null);
    }
    
}