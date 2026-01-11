using App.Domain;

namespace App.DTO;

public class SchoolDto(SchoolEntity school, string bucketUrl)
{
    public Guid Id { get; set; } = school.Id;
    public string Name { get; set; } = school.Name;
    public string ShortName { get; set; } = school.ShortName;
    public string Domain { get; set; } = school.Domain;
    public string StudentCodePattern { get; set; } = school.StudentCodePattern;
    public string? PhotoLink { get; set; } = school.PhotoPath != string.Empty ? bucketUrl + school.PhotoPath : null;
    
    public static List<SchoolDto> ToDtoList(List<SchoolEntity>? entities, string bucketUrl)
    {
        if (entities == null)
        {
            return new List<SchoolDto>();
        }
        return entities.Select(e => new SchoolDto(e, bucketUrl)).ToList();
    }
}