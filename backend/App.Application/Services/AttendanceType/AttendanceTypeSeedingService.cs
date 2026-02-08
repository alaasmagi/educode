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
        var attendanceTypes = new List<AttendanceTypeEntity>
        {
            new()
            {
                TypeName = "lecture",
                CreatedBy = Constants.BackendName,
                CreatedByClient = Constants.BackendName,
                UpdatedBy = Constants.BackendName,
                UpdatedByClient = Constants.BackendName,
            },
            new()
            {
                TypeName = "practice",
                CreatedBy = Constants.BackendName,
                CreatedByClient = Constants.BackendName,
                UpdatedBy = Constants.BackendName,
                UpdatedByClient = Constants.BackendName,
            },
            new()
            {
                TypeName = "lecture-practice",
                CreatedBy = Constants.BackendName,
                CreatedByClient = Constants.BackendName,
                UpdatedBy = Constants.BackendName,
                UpdatedByClient = Constants.BackendName,
            }
        };

        foreach (var attendanceType in attendanceTypes)
        {
            var existing = await attendanceTypeRepository.SearchAsync(attendanceType.TypeName);
            if (existing != null && existing.Any(at => at.TypeName == attendanceType.TypeName))
            {
                logger.LogInformation($"Attendance type {attendanceType.TypeName} already exists, skipping");
                continue;
            }

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