using App.Contracts.DTOs;
using App.Domain.Entities;
using Base.DTO;

namespace App.Contracts.Services;

public interface IUserService
{
    Task<MethodResponse<List<UserDto>>> GetAllUsersAsync(int pageNr, int pageSize);
    Task<MethodResponse<UserDto>> GetUserByIdAsync(Guid id);
    Task<MethodResponse<bool>> UpdateUserAsync(UserEntity user);
    Task<MethodResponse<bool>> SoftDeleteUserAsync(Guid userId);
    Task<MethodResponse<bool>> RestoreUserAsync(Guid userId);
}

