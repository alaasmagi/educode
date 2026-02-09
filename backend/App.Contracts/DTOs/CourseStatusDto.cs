using App.Domain.Entities;

namespace App.Contracts.DTOs;

public class CourseStatusDto
{
    // Parameterless constructor for deserialization
    public CourseStatusDto() { }
    
    public CourseStatusDto(CourseStatusEntity courseStatus)
    {
        Id = courseStatus.Id;
        Status = courseStatus.StatusName;
    }

    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    
    public static List<CourseStatusDto> ToDtoList(List<CourseStatusEntity>? entities)
    {
        return entities?.Select(e => new CourseStatusDto(e)).ToList() 
               ?? new List<CourseStatusDto>();
    }
}