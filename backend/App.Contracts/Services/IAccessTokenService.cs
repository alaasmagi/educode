using App.Domain.Entities;

namespace App.Contracts.Services;

public interface IAccessTokenService
{
    string GenerateAccessToken(UserEntity user, UserAuthEntity? userAuth);
}