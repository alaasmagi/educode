namespace App.Contracts.WebRequests;

public class AttendanceCheckRequest : BaseRequest
{
    public required string StudentCode { get; set; }
    public required string FullName { get; set; }
    public required string CourseAttendanceIdentifier { get; set; }
    public string? WorkplaceIdentifier { get; set; }
}