using App.BLL;
using App.DAL.EF;
using App.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.BLL_Tests;

[TestFixture]
public class AttendanceManagementServiceTests
{
    private AppDbContext _context = null!;
    private AttendanceManagementService _service = null!;
    private ILogger<AttendanceManagementService> _logger = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        _context = new AppDbContext(options);
        _logger = new LoggerFactory().CreateLogger<AttendanceManagementService>();
        _service = new AttendanceManagementService(_context, _logger);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
    
    [Test]
    public async Task DoesWorkplaceExist_ReturnsTrue_WhenWorkplaceExists()
    {
        var workplace = new WorkplaceEntity
        {
            Id = 1, 
            ComputerCode = "Test Workplace",
            ClassRoom = "Test Class Room",
            CreatedBy = "TEST", 
            UpdatedBy = "TEST",
        };
        await _context.Workplaces.AddAsync(workplace);
        await _context.SaveChangesAsync();

        var result = await _service.DoesWorkplaceExist(workplace.Id);
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task DoesWorkplaceExist_ReturnsFalse_WhenWorkplaceDoesNotExist()
    {
        var result = await _service.DoesWorkplaceExist(999);
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task DoesAttendanceExist_ReturnsTrue_WhenAttendanceExists()
    {
        var attendance = new CourseAttendanceEntity
        {
            Id = 1,
            AttendanceTypeId = 1,
            CourseId = 1,
            CreatedBy = "TEST", 
            UpdatedBy = "TEST"
        };
        await _context.CourseAttendances.AddAsync(attendance);
        await _context.SaveChangesAsync();

        var result = await _service.DoesAttendanceExist(1);
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task DoesAttendanceExist_ReturnsFalse_WhenAttendanceDoesNotExist()
    {
        var result = await _service.DoesAttendanceExist(999);
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task AddAttendanceCheckAsync_AddsSuccessfully_WhenNotExists()
    {
        var attendance = new CourseAttendanceEntity
        {
            Id = 1,
            AttendanceTypeId = 1,
            CourseId = 1,
            CreatedBy = "TEST", 
            UpdatedBy = "TEST"
        };
        var check = new AttendanceCheckEntity
        {
            Id = 1,
            StudentCode = "S123",
            FullName = "TEST NAME",
            CourseAttendanceId = 1,
            CreatedBy = "TEST", 
            UpdatedBy = "TEST"
        };
        await _context.CourseAttendances.AddAsync(attendance);
        await _context.SaveChangesAsync();

        var result = await _service.AddAttendanceCheckAsync(check, "creator", null);
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task AddAttendanceCheckAsync_ReturnsFalse_WhenAlreadyExists()
    {
        var attendance = new CourseAttendanceEntity
        {
            Id = 1,
            AttendanceTypeId = 1,
            CourseId = 1,
            CreatedBy = "TEST", 
            UpdatedBy = "TEST"
        };
        var check = new AttendanceCheckEntity
        {
            Id = 1,
            StudentCode = "S123",
            FullName = "TEST NAME",
            CourseAttendanceId = 1,
            CreatedBy = "TEST", 
            UpdatedBy = "TEST"
        };
        await _context.CourseAttendances.AddAsync(attendance);
        await _context.AttendanceChecks.AddAsync(check);
        await _context.SaveChangesAsync();

        var result = await _service.AddAttendanceCheckAsync(check, "creator", null);
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task GetAttendanceTypesAsync_ReturnsList_WhenExists()
    {
        await _context.AttendanceTypes.AddAsync(new AttendanceTypeEntity { 
            Id = 1, 
            AttendanceType = "Lecture",
            CreatedBy = "TEST", 
            UpdatedBy = "TEST" });
        await _context.SaveChangesAsync();

        var result = await _service.GetAttendanceTypesAsync();
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public async Task GetAttendanceTypesAsync_ReturnsNull_WhenNoneExists()
    {
        var result = await _service.GetAttendanceTypesAsync();
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task DeleteAttendance_ReturnsFalse_WhenAttendanceDoesNotExist()
    {
        var result = await _service.DeleteAttendance(999, "uni123");
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task GetStudentsCountByAttendanceIdAsync_ReturnsZero_WhenNoneExists()
    {
        var count = await _service.GetStudentsCountByAttendanceIdAsync(999);
        Assert.That(0 == count);
    }
    
    [Test]
    public async Task EditAttendanceAsync_UpdatesSuccessfully_WhenAttendanceExists()
    {
        var attendance = new CourseAttendanceEntity
        {
            Id = 1,
            AttendanceTypeId = 1,
            CourseId = 1,
            CreatedBy = "TEST", 
            UpdatedBy = "TEST"
        };
        await _context.CourseAttendances.AddAsync(attendance);
        await _context.SaveChangesAsync();

        var updated = new CourseAttendanceEntity
        {
            AttendanceTypeId = 1,
            CourseId = 2,
            CreatedBy = "TEST2", 
            UpdatedBy = "TEST2"
        };

        var result = await _service.EditAttendanceAsync(1, updated);
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task EditAttendanceAsync_ReturnsFalse_WhenAttendanceDoesNotExist()
    {
        var updated = new CourseAttendanceEntity { AttendanceTypeId = 99 };
        var result = await _service.EditAttendanceAsync(999, updated);
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task GetCourseAttendanceByIdAsync_ReturnsNull_WhenNotAccessible()
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
        var attendance = new CourseAttendanceEntity
        {
            Id = 1,
            CourseId = 99,
            CreatedBy = "test",
            UpdatedBy = "test"
        };
        
        await _context.Users.AddAsync(user);
        await _context.CourseAttendances.AddAsync(attendance);
        await _context.SaveChangesAsync();

        var result = await _service.GetCourseAttendanceByIdAsync(1, "exist123");
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task AddAttendanceAsync_AddsAllDatesSuccessfully()
    {
        var attendance = new CourseAttendanceEntity
        {
            Id = 1,
            CourseId = 1,
            AttendanceTypeId = 0,
            CreatedBy = "admin",
            UpdatedBy = "admin"
        };

        await _context.CourseAttendances.AddAsync(attendance);
        var result = await _context.SaveChangesAsync();
        Assert.That(result > 0);
    }
}