using App.Contracts.Repositories;
using App.Contracts.Services;
using App.Domain.Entities;
using App.Infrastructure.Helpers;
using Base.Domain;
using Base.DTO;
using Microsoft.Extensions.Logging;

namespace App.Application.Services.Course;

public class CourseStatusSeedingService(
    ILogger<CourseStatusSeedingService> logger,
    ICourseStatusRepository courseStatusRepository) : ICourseStatusSeedingService
{
    public async Task<MethodResponse<bool>> Seed()
    {
        var courseStatuses = new List<CourseStatusEntity>
        {
            new()
            {
                StatusName = "available",
                CreatedBy = Constants.BackendName,
                CreatedByClient = Constants.BackendName,
                UpdatedBy = Constants.BackendName,
                UpdatedByClient = Constants.BackendName,
            },
            new()
            {
                StatusName = "unavailable",
                CreatedBy = Constants.BackendName,
                CreatedByClient = Constants.BackendName,
                UpdatedBy = Constants.BackendName,
                UpdatedByClient = Constants.BackendName,
            },
            new()
            {
                StatusName = "temp-unavailable",
                CreatedBy = Constants.BackendName,
                CreatedByClient = Constants.BackendName,
                UpdatedBy = Constants.BackendName,
                UpdatedByClient = Constants.BackendName,
            }
        };
        
        foreach (var courseStatus in courseStatuses)
        {
            var result = await courseStatusRepository.CreateAsync(courseStatus);

            if (result == null)
            {
                logger.LogWarning("Failed to seed course statuses");
                return MethodResponse<bool>.Failure(
                    new Error(ErrorConstants.CourseStatusesNotSeeded, "Course statuses were not seeded")
                );
            }
        }

        logger.LogInformation("Successfully seeded course statuses");
        return MethodResponse<bool>.Success(true);
    }  
}