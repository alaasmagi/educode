using App.Domain.Enums;
using Base.Domain;

namespace App.Domain.Entities;

public class UserTypeEntity : BaseEntity
{
    public string TypeName { get; set; } = default!;
    public EAccessLevel AccessLevel { get; set; } = EAccessLevel.NoAccess;
}