using App.DAL.EF;
using Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace App.BLL;

public class CleanupService : ICleanupService
{
    private readonly ILogger<CleanupService> _logger;
    private readonly AttendanceRepository _attendanceRepository;
    private readonly UserRepository _userRepository;
    private readonly CourseRepository _courseRepository;
    private readonly int _softDeleteExpirationDays;
    private readonly DateTime _datePeriod;
    private Timer? _timer;

    public CleanupService(ILogger<CleanupService> logger, IServiceScopeFactory scopeFactory, EnvInitializer initializer)
    {
        _logger = logger;
        _timer = null;
        var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _attendanceRepository = new AttendanceRepository(dbContext);
        _userRepository = new UserRepository(dbContext);
        _courseRepository = new CourseRepository(dbContext);
        _softDeleteExpirationDays = initializer.SoftDeleteExpirationDays;
        _datePeriod = DateTime.UtcNow.AddDays(-_softDeleteExpirationDays);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var timeToRun = DateTime.Today.AddDays(1).AddHours(3) - DateTime.UtcNow;
        _timer = new Timer(ExecuteTask!, null, timeToRun, TimeSpan.FromDays(1));
        _logger.LogInformation("Cleanup service started.");
        
        _ = Task.Run(() => ExecuteTask(default!), cancellationToken);
        
        return Task.CompletedTask;
    }

    private async void ExecuteTask(object state)
    {
        try
        {
            await HardDeleteOldUserEntitiesAsync();
            await HardDeleteOldSchoolEntitiesAsync();
            await HardDeleteOldCourseAttendanceEntitiesAsync();
            await HardDeleteOldCourseTeacherEntitiesAsync();
            await HardDeleteOldAttendanceCheckEntitiesAsync();
            await HardDeleteOldUserAuthEntitiesAsync();
            await HardDeleteOldCourseEntitiesAsync();
            await HardDeleteOldUserTypeEntitiesAsync();
            await HardDeleteOldCourseStatusEntitiesAsync();
            await HardDeleteOldAttendanceTypeEntitiesAsync();
            await HardDeleteOldWorkplaceEntitiesAsync();
            await HardDeleteOldRefreshTokenEntitiesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred during the attendance cleanup task: {ex.Message}");
        }
    }

    public async Task HardDeleteOldCourseEntitiesAsync()
    {
        var status = await _courseRepository.RemoveOldCourses(_datePeriod);

        if (!status)
        {
            _logger.LogInformation($"Found no courses to delete that are more than {_softDeleteExpirationDays} days old");
            return;
        }

        _logger.LogInformation($"Successfully deleted courses that were more than {_softDeleteExpirationDays} days old");
    }
    
    public async Task HardDeleteOldUserEntitiesAsync()
    {
        var status = await _userRepository.RemoveOldUsers(_datePeriod);

        if (!status)
        {
            _logger.LogInformation($"Found no users to delete that are more than {_softDeleteExpirationDays} days old");
            return;
        }

        _logger.LogInformation($"Successfully deleted users that were more than {_softDeleteExpirationDays} days old");
    }
    
    public async Task HardDeleteOldUserAuthEntitiesAsync()
    {
        var status = await _userRepository.RemoveOldUserAuths(_datePeriod);

        if (!status)
        {
            _logger.LogInformation($"Found no user auths to delete that are more than {_softDeleteExpirationDays} days old");
            return;
        }

        _logger.LogInformation($"Successfully deleted user auths that were more than {_softDeleteExpirationDays} days old");
    }
    
    public async Task HardDeleteOldAttendanceCheckEntitiesAsync()
    {
        var status = await _attendanceRepository.RemoveOldAttendanceChecks(_datePeriod);

        if (!status)
        {
            _logger.LogInformation($"Found no attendance checks to delete that are more than {_softDeleteExpirationDays} days old");
            return;
        }

        _logger.LogInformation($"Successfully deleted attendance checks that were more than {_softDeleteExpirationDays} days old");
    }
    
    public async Task HardDeleteOldCourseTeacherEntitiesAsync()
    {
        var status = await _courseRepository.RemoveOldCourseTeachers(_datePeriod);

        if (!status)
        {
            _logger.LogInformation($"Found no course teachers to delete that are more than {_softDeleteExpirationDays} days old");
            return;
        }

        _logger.LogInformation($"Successfully deleted course teachers that were more than {_softDeleteExpirationDays} days old");
    }
    
    public async Task HardDeleteOldSchoolEntitiesAsync()
    {
        var status = await _userRepository.RemoveOldSchools(_datePeriod);

        if (!status)
        {
            _logger.LogInformation($"Found no schools to delete that are more than {_softDeleteExpirationDays} days old");
            return;
        }
        
        _logger.LogInformation($"Successfully deleted schools that were more than {_softDeleteExpirationDays} days old");
    }
    
    public async Task HardDeleteOldCourseAttendanceEntitiesAsync()
    {
        var status = await _attendanceRepository.RemoveOldAttendances(_datePeriod);

        if (!status)
        {
            _logger.LogInformation($"Found no course attendances to delete that are more than {_softDeleteExpirationDays} days old");
            return;
        }
        
        _logger.LogInformation($"Successfully deleted course attendances that were more than {_softDeleteExpirationDays} days old");
    }
    
    public async Task HardDeleteOldUserTypeEntitiesAsync()
    {
        var status = await _userRepository.RemoveOldUserTypes(_datePeriod);

        if (!status)
        {
            _logger.LogInformation($"Found no user types to delete that are more than {_softDeleteExpirationDays} days old");
            return;
        }
        
        _logger.LogInformation($"Successfully deleted user types that were more than {_softDeleteExpirationDays} days old");
    }
    
    public async Task HardDeleteOldCourseStatusEntitiesAsync()
    {
        var status = await _courseRepository.RemoveOldCourseStatuses(_datePeriod);

        if (!status)
        {
            _logger.LogInformation($"Found no course statuses to delete that are more than {_softDeleteExpirationDays} days old");
            return;
        }
        
        _logger.LogInformation($"Successfully deleted course statuses that were more than {_softDeleteExpirationDays} days old");
    }
    
    public async Task HardDeleteOldWorkplaceEntitiesAsync()
    {
        var status = await _attendanceRepository.RemoveOldWorkplaces(_datePeriod);

        if (!status)
        {
            _logger.LogInformation($"Found no workplaces to delete that are more than {_softDeleteExpirationDays} days old");
            return;
        }
        
        _logger.LogInformation($"Successfully deleted workplaces that were more than {_softDeleteExpirationDays} days old");
    }
    
    public async Task HardDeleteOldAttendanceTypeEntitiesAsync()
    {
        var status = await _attendanceRepository.RemoveOldAttendanceTypes(_datePeriod);

        if (!status)
        {
            _logger.LogInformation($"Found no attendance types to delete that are more than {_softDeleteExpirationDays} days old");
            return;
        }
        
        _logger.LogInformation($"Successfully deleted attendance types that were more than {_softDeleteExpirationDays} days old");
    }
    
    
    public async Task HardDeleteOldRefreshTokenEntitiesAsync()
    {
        var status = await _userRepository.RemoveOldRefreshTokens(_datePeriod);

        if (!status)
        {
            _logger.LogInformation($"Found no refresh tokens to delete that are more than {_softDeleteExpirationDays} days old");
            return;
        }
        
        _logger.LogInformation($"Successfully deleted refresh tokens that were more than {_softDeleteExpirationDays} days old");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        _logger.LogInformation("Attendance cleanup service stopped.");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _logger.LogInformation("Attendance cleanup service disposed.");
    }
}