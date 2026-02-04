using App.Application.Contracts.Services;
using App.Contracts.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace App.Application.Initializers;

// TODO: Implement proper error logging (sentry)
public class DbInitializer(ILogger<DbInitializer> logger, ISentryService sentry, IServiceScopeFactory scopeFactory)
{
    public async Task InitializeDb()
    {
        using (var scope = scopeFactory.CreateScope())
        {
            var courseService = scope.ServiceProvider.GetRequiredService<ICourseManagementService>();
            var attendanceService = scope.ServiceProvider.GetRequiredService<IAttendanceManagementService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserManagementService>();

            await attendanceService.SeedAttendanceTypes();
            await courseService.SeedCourseStatuses();
            await userService.SeedUserTypes();
            await userService.SeedAdminUser();
        }
        
        logger.LogInformation("Database initialization completed.");
    }
}