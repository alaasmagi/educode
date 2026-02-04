using App.Domain.Entities;

namespace App.Application.Contracts.Services;

public interface IUserManagementService
{
    Task<UserEntity?> AuthenticateUserAsync(string email, string password);
    Task<bool> CreateAccountAsync(UserEntity user, UserAuthEntity userAuthData);
    Task<bool> ChangeUserPasswordAsync(UserEntity user, string newPasswordHash);
    Task<UserEntity?> GetUserByEmailAsync(string email);
    Task<UserTypeEntity?> GetUserTypeAsync(string userType);
    Task<List<UserEntity>?> GetAllUsersAsync(int pageNr, int pageSize);
    Task<UserEntity?> GetUserByIdAsync(Guid id);
    Task<bool> SoftDeleteUserAsync(UserEntity user);
    Task<bool> UpdateUserAsync(UserEntity user);
    Task<bool> SeedUserTypes();
    Task<bool> SeedAdminUser();

}