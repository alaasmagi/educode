using App.Domain;

namespace App.DTO;

public class UserDto(UserEntity user, string bucketUrl)
{
    public Guid Id { get; set; } = user.Id;
    public string Email { get; set; } = user.Email;
    public Guid UserTypeId { get; set; } = user.UserTypeId;
    public string? UserType { get; set; } = user.UserType?.UserType ?? null;
    public EAccessLevel? AccessLevel { get; set; } = user.UserType?.AccessLevel ?? null;
    public string? StudentCode { get; set; } = user.StudentCode ?? null;
    public string? PhotoLink { get; set; } = user.PhotoPath != string.Empty ? bucketUrl + user.PhotoPath : null;
    
    public static List<UserDto> ToDtoList(List<UserEntity>? entities, string bucketUrl)
    {
        if (entities == null)
        {
            return new List<UserDto>();
        }
        return entities.Select(e => new UserDto(e, bucketUrl)).ToList();
    }
}