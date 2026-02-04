namespace App.Domain.Enums;

public enum EAccessLevel
{
    NoAccess,
    PrimaryLevel,       // For students
    SecondaryLevel,     // For teacher assistants
    TertiaryLevel,      // For teachers
    QuaternaryLevel,    // For managers
    QuinaryLevel        // For admins
}