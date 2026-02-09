using App.Domain.Entities;

namespace App.Contracts.DTOs;

public class SchoolDto
{
    // Parameterless constructor for deserialization
    public SchoolDto() { }
    
    public SchoolDto(SchoolEntity school)
    {
        Id = school.Id;
        Name = school.Name;
        ShortName = school.ShortName;
        Domain = school.Domain;
        StudentCodePattern = school.StudentCodePattern;
    }

    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string StudentCodePattern { get; set; } = string.Empty;
    
    public static List<SchoolDto> ToDtoList(List<SchoolEntity>? entities)
    {
        return entities?.Select(e => new SchoolDto(e)).ToList() 
               ?? new List<SchoolDto>();
    }
}