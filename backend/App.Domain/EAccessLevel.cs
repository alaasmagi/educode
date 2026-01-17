namespace App.Domain;

public enum EAccessLevel
{
    NoAccess,
    PrimaryLevel,       // For students
    SecondaryLevel,     // For teacher assistants
    TertiaryLevel,      // For teachers
    QuaternaryLevel,    // For managers
    QuinaryLevel        // For admins
}