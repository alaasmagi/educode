using App.Contracts.DTOs;
using App.Domain.Entities;

namespace App.Contracts.Services;

public interface IAccessTokenService
{
    string GenerateAccessToken(UserEntity user, UserAuthEntity userAuth, string clientApp);
    string GenerateAdminAccessToken(UserDto user);
}