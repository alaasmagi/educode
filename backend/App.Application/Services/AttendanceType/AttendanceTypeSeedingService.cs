using App.Contracts.Repositories;
using App.Contracts.Services;
using App.Domain.Entities;
using App.Infrastructure.Helpers;
using Base.Domain;
using Base.DTO;
using Microsoft.Extensions.Logging;

namespace App.Application.Services.AttendanceType;

public class AttendanceTypeSeedingService(
    IAttendanceTypeRepository attendanceTypeRepository,
    ILogger<AttendanceTypeSeedingService> logger) : IAttendanceTypeSeedingService
{
    public async Task<MethodResponse<bool>> Seed()
    {
        var now = DateTime.UtcNow;

        var attendanceTypes = new List<AttendanceTypeEntity>
        {
            new()
            {
                TypeName = "lecture",
                CreatedBy = "aspnet-initializer",
                CreatedByClient = "aspnet-initializer",
                CreatedAt = now,
                UpdatedBy = "aspnet-initializer",
                UpdatedByClient = "aspnet-initializer",
                UpdatedAt = now,
            },
            new()
            {
                TypeName = "practice",
                CreatedBy = "aspnet-initializer",
                CreatedByClient = "aspnet-initializer",
                CreatedAt = now,
                UpdatedBy = "aspnet-initializer",
                UpdatedByClient = "aspnet-initializer",
                UpdatedAt = now,
            },
            new()
            {
                TypeName = "lecture-practice",
                CreatedBy = "aspnet-initializer",
                CreatedByClient = "aspnet-initializer",
                CreatedAt = now,
                UpdatedBy = "aspnet-initializer",
                UpdatedByClient = "aspnet-initializer",
                UpdatedAt = now,
            }
        };

        foreach (var attendanceType in attendanceTypes)
        {
            var result = await attendanceTypeRepository.CreateAsync(attendanceType);

            if (result == null)
            {
                logger.LogWarning("Failed to seed attendance types");
                return MethodResponse<bool>.Failure(
                    new Error(ErrorConstants.AttendanceTypesNotSeeded, "Attendance types were not seeded")
                );
            }
        }

        logger.LogInformation("Successfully seeded attendance types");
        return MethodResponse<bool>.Success(true);
    }
}