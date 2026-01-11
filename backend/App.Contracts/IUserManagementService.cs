using App.Domain;

namespace Contracts;

public interface IUserManagementService
{
    Task<UserEntity?> AuthenticateUserAsync(Guid userId, string password);
    Task<bool> CreateAccountAsync(UserEntity user, UserAuthEntity userAuthData);
    Task<bool> ChangeUserPasswordAsync(UserEntity user, string newPasswordHash);
    Task<UserEntity?> GetUserByEmailAsync(string email);
    Task<UserTypeEntity?> GetUserTypeAsync(string userType);
    Task<List<UserEntity>?> GetAllUsersAsync(int pageNr, int pageSize);
    Task<bool> DoesUserExistAsync(string email);
    Task<UserEntity?> GetUserByIdAsync(Guid id);
    string GetPasswordHash(string password);
    Task<bool> DeleteUserAsync(UserEntity user);
    Task<bool> UpdateUserAsync(UserEntity user);

}