using App.Domain.Entities;

namespace App.Contracts.DTOs;

public class CourseDto
{
    // Parameterless constructor for deserialization
    public CourseDto() { }
    
    public CourseDto(CourseEntity course)
    {
        Id = course.Id;
        CourseCode = course.Code;
        CourseName = course.Name;
        CourseStatusId = course.StatusId;
        CourseStatus = course.Status?.StatusName;
    }

    public Guid Id { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public Guid? CourseStatusId { get; set; }
    public string? CourseStatus { get; set; }
    
    public static List<CourseDto> ToDtoList(List<CourseEntity>? entities)
    {
        return entities?.Select(e => new CourseDto(e)).ToList() 
               ?? new List<CourseDto>();
    }
}