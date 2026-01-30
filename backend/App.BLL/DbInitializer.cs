using App.BLL.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace App.Common;

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
        var userService = scope.ServiceProvider.GetRequiredService<IUserManagementService>();
        var courseService = scope.ServiceProvider.GetRequiredService<ICourseManagementService>();
        var attendanceService = scope.ServiceProvider.GetRequiredService<IAttendanceManagementService>();

        attendanceService.SeedAttendanceTypes();
        courseService.SeedCourseStatuses();
        userService.SeedUserTypes();
        userService.SeedAdminUser();
        
        _logger.LogInformation("Database initialization completed.");
    }
}