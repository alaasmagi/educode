using App.DAL.EF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace App.BLL;

public class DbInitializer
{
    private readonly ILogger<DbInitializer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public DbInitializer(ILogger<DbInitializer> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public void InitializeDb()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var attendanceRepository = new AttendanceRepository(context);
        var courseRepository = new CourseRepository(context);
        var userRepository = new UserRepository(context);

        attendanceRepository.SeedAttendanceTypes();
        courseRepository.SeedCourseStatuses();
        userRepository.SeedUserTypes();

        _logger.LogInformation("Database initialization completed.");
    }
}