using App.Domain;

namespace App.DTO;

public class CourseDto(CourseEntity course)
{
    public Guid Id { get; set; } = course.Id;
    public string CourseCode { get; set; } = course.CourseCode;
    public string CourseName { get; set; } = course.CourseName;
    public Guid? CourseStatusId { get; set; } = course.CourseStatusId;
    public string? CourseStatus { get; set; } = course.CourseStatus?.CourseStatus;
    
    public static List<CourseDto> ToDtoList(List<CourseEntity>? entities)
    {
        if (entities == null)
        {
            return new List<CourseDto>();
        }
        return entities.Select(e => new CourseDto(e)).ToList();
    }
}