using App.Domain.Entities;

namespace App.Contracts.DTOs;

public class SchoolDto(SchoolEntity school)
{
    public Guid Id { get; set; } = school.Id;
    public string Name { get; set; } = school.Name;
    public string ShortName { get; set; } = school.ShortName;
    public string Domain { get; set; } = school.Domain;
    public string StudentCodePattern { get; set; } = school.StudentCodePattern;
    
    public static List<SchoolDto> ToDtoList(List<SchoolEntity>? entities)
    {
        if (entities == null)
        {
            return new List<SchoolDto>();
        }
        return entities.Select(e => new SchoolDto(e)).ToList();
    }
}