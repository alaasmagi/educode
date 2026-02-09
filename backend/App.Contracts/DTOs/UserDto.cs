using App.Domain.Entities;
using App.Domain.Enums;

namespace App.Contracts.DTOs;

public class UserDto
{
    // Parameterless constructor for deserialization
    public UserDto() { }
    
    public UserDto(UserEntity user, string bucketUrl)
    {
        Id = user.Id;
        Email = user.Email;
        UserTypeId = user.TypeId;
        UserType = user.Type?.TypeName;
        AccessLevel = user.Type?.AccessLevel;
        StudentCode = user.StudentCode;
        PhotoLink = user.PhotoPath != null ? bucketUrl + user.PhotoPath : null;
    }

    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public Guid UserTypeId { get; set; }
    public string? UserType { get; set; }
    public EAccessLevel? AccessLevel { get; set; }
    public string? StudentCode { get; set; }
    public string? PhotoLink { get; set; }
    
    public static List<UserDto> ToDtoList(List<UserEntity>? entities, string bucketUrl)
    {
        return entities?.Select(e => new UserDto(e, bucketUrl)).ToList() 
               ?? new List<UserDto>();
    }
}