namespace App.Contracts.WebRequests;

public class AttendanceRequest
{
    public required Guid CourseId {get; set;}
    public required Guid ClassroomId {get; set;}
    public required Guid AttendanceTypeId { get; set; }
    public required TimeOnly StartTime  { get; set; }
    public required TimeOnly EndTime  { get; set; }
    public required List<DateOnly> AttendanceDates { get; set; }
    public required bool AutomatedRegistration { get; set; }
}