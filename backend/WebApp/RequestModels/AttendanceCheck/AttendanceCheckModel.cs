namespace WebApp.RequestModels.AttendanceCheck;

public class AttendanceCheckModel : BaseModel
{
    public required string StudentCode { get; set; }
    public required string FullName { get; set; }
    public required string CourseAttendanceIdentifier { get; set; }
    public string? WorkplaceIdentifier { get; set; }
}