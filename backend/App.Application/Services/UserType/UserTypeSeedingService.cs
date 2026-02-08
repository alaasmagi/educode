using App.Contracts.Repositories;
using App.Contracts.Services;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Infrastructure.Helpers;
using Base.DTO;
using Microsoft.Extensions.Logging;

namespace App.Application.Services.UserType;

public class UserTypeSeedingService(
    IUserTypeRepository userTypeRepository,
    ILogger<UserTypeSeedingService> logger) : IUserTypeSeedingService
{
     public async Task<MethodResponse<bool>> Seed()
    {   
        var userTypes = new List<UserTypeEntity>
        {
            new UserTypeEntity
            {
                TypeName = "student",
                AccessLevel = EAccessLevel.PrimaryLevel,
                CreatedBy = Constants.BackendName,
                UpdatedBy = Constants.BackendName,
            },
            new UserTypeEntity
            {
                TypeName = "teacher-assistant",
                AccessLevel = EAccessLevel.SecondaryLevel,
                CreatedBy = Constants.BackendName,
                UpdatedBy = Constants.BackendName,
            },
            new UserTypeEntity
            {
                TypeName = "teacher",
                AccessLevel = EAccessLevel.TertiaryLevel,
                CreatedBy = Constants.BackendName,
                UpdatedBy = Constants.BackendName,
            },
            new UserTypeEntity
            {
                TypeName = "school-administrator",
                AccessLevel = EAccessLevel.QuaternaryLevel,
                CreatedBy = Constants.BackendName,
                UpdatedBy = Constants.BackendName,
            },
            new UserTypeEntity
            {
                TypeName = "system-administrator",
                AccessLevel = EAccessLevel.QuinaryLevel,
                CreatedBy = Constants.BackendName,
                UpdatedBy = Constants.BackendName,
            }
        };

        foreach (var userType in userTypes)
        {
            var result = await userTypeRepository.CreateAsync(userType);

            if (result == null)
            {
                logger.LogError($"Failed to seed user type {userType.TypeName}");
                return MethodResponse<bool>.Failure(
                    new Error(ErrorConstants.UserTypesNotSeeded, "User types were not seeded")
                );
            }
        }

        logger.LogInformation("Successfully seeded user types");
        return MethodResponse<bool>.Success(true);
    }
}