using Microsoft.Extensions.Hosting;
namespace Contracts;


public interface ICleanupService : IHostedService, IDisposable
{
    Task HardDeleteOldAttendanceCheckEntitiesAsync();
    Task HardDeleteOldUserAuthEntitiesAsync();
    Task HardDeleteOldUserEntitiesAsync();
    Task HardDeleteOldCourseTeacherEntitiesAsync();
    Task HardDeleteOldRefreshTokenEntitiesAsync();
    Task HardDeleteOldAttendanceTypeEntitiesAsync();
    Task HardDeleteOldWorkplaceEntitiesAsync();
    Task HardDeleteOldCourseStatusEntitiesAsync();
    Task HardDeleteOldUserTypeEntitiesAsync();
    Task HardDeleteOldCourseAttendanceEntitiesAsync();
    Task HardDeleteOldSchoolEntitiesAsync();
}