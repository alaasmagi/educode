using App.Domain;

namespace App.DTO;

public class CourseDto(CourseEntity course)
{
    public Guid Id { get; set; } = course.Id;
    public string CourseCode { get; set; } = course.Code;
    public string CourseName { get; set; } = course.Name;
    public Guid? CourseStatusId { get; set; } = course.StatusId;
    public string? CourseStatus { get; set; } = course.Status?.StatusName;
    
    public static List<CourseDto> ToDtoList(List<CourseEntity>? entities)
    {
        if (entities == null)
        {
            return new List<CourseDto>();
        }
        return entities.Select(e => new CourseDto(e)).ToList();
    }
}