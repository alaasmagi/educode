using System.ComponentModel.DataAnnotations;

namespace Base.Domain;

public abstract class BaseEntity
{
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(128)]
    public string CreatedBy { get; set; } = default!;
    
    [Required]
    [MaxLength(128)]
    public string CreatedByClient { get; set; } = default!;

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    [MaxLength(128)]
    public string UpdatedBy { get; set; } = default!;
    
    [Required]
    [MaxLength(128)]
    public string UpdatedByClient { get; set; } = default!;

    [Required]
    public DateTime UpdatedAt { get; set; }

    [Required]
    public bool Deleted { get; set; } = false;

    public virtual List<Error> Validate()
    {
        var errors = new List<Error>();

        if (Id == Guid.Empty)
        {
            errors.Add(new Error("id-empty", "Id cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(CreatedBy))
        {
            errors.Add(new Error("created-by-empty", "CreatedBy cannot be empty"));
        }
        else if (CreatedBy.Length > 128)
        {
            errors.Add(new Error("created-by-too-long", "CreatedBy cannot exceed 128 characters"));
        }
        
        if (string.IsNullOrWhiteSpace(CreatedByClient))
        {
            errors.Add(new Error("created-by-client-empty", "CreatedBy cannot be empty"));
        }
        else if (CreatedByClient.Length > 128)
        {
            errors.Add(new Error("created-by-client-too-long", "CreatedBy cannot exceed 128 characters"));
        }

        if (string.IsNullOrWhiteSpace(UpdatedBy))
        {
            errors.Add(new Error("updated-by-empty", "UpdatedBy cannot be empty"));
        }
        else if (UpdatedBy.Length > 128)
        {
            errors.Add(new Error("updated-by-too-long", "UpdatedBy cannot exceed 128 characters"));
        }
        
        if (string.IsNullOrWhiteSpace(UpdatedByClient))
        {
            errors.Add(new Error("updated-by-client-empty", "UpdatedBy cannot be empty"));
        }
        else if (UpdatedByClient.Length > 128)
        {
            errors.Add(new Error("updated-by-client-too-long", "UpdatedBy cannot exceed 128 characters"));
        }

        return errors;
    }
}