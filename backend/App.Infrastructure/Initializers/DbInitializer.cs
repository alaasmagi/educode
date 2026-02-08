using App.Contracts.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.Initializers;

// TODO: Implement proper error logging (sentry)
public class DbInitializer(ILogger<DbInitializer> logger, ISentryService sentry, IServiceScopeFactory scopeFactory)
{
    public async Task InitializeDb()
    {
        using (var scope = scopeFactory.CreateScope())
        {
            var userTypeSeedingService = scope.ServiceProvider.GetRequiredService<IUserTypeSeedingService>();
            var userSeedingService = scope.ServiceProvider.GetRequiredService<IUserSeedingService>();
            var attendanceTypeSeedingService = scope.ServiceProvider.GetRequiredService<IAttendanceTypeSeedingService>();
            var courseStatusSeedingService = scope.ServiceProvider.GetRequiredService<ICourseStatusSeedingService>();

            await userTypeSeedingService.Seed();
            await userSeedingService.Seed();
            await attendanceTypeSeedingService.Seed();
            await courseStatusSeedingService.Seed();
        }
        
        logger.LogInformation("Database initialization completed.");
    }
}