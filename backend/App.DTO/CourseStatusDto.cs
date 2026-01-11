using App.Domain;

namespace App.DTO;

public class CourseStatusDto(CourseStatusEntity courseStatus)
{
    public Guid Id { get; set; } = courseStatus.Id;
    public string Status { get; set; } = courseStatus.CourseStatus;
    
    public static List<CourseStatusDto> ToDtoList(List<CourseStatusEntity>? entities)
    {
        if (entities == null)
        {
            return new List<CourseStatusDto>();
        }
        return entities.Select(e => new CourseStatusDto(e)).ToList();
    }
}