namespace App.Contracts.WebRequests;

public class AttendanceChangeRequest
{
    public required Guid ClassroomId {get; set;}
    public required Guid AttendanceTypeId { get; set; }
    public required DateTime StartTime  { get; set; }
    public required DateTime EndTime  { get; set; }
    public required bool AutomatedRegistration { get; set; }
}