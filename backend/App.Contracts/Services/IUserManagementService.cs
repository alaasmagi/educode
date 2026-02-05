using App.Domain.Entities;

namespace App.Contracts.Services;

public interface IUserManagementService
{
    Task<UserTypeEntity?> GetUserTypeAsync(string userType);
    Task<List<UserEntity>?> GetAllUsersAsync(int pageNr, int pageSize);
    Task<UserEntity?> GetUserByIdAsync(Guid id);
    Task<bool> SoftDeleteUserAsync(Guid userId);
    Task<bool> RestoreUserAsync(Guid userId);
    Task<bool> UpdateUserAsync(UserEntity user);
    Task<bool> SeedUserTypes();
    Task<bool> SeedAdminUser();

}