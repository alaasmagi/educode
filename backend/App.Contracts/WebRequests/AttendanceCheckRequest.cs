namespace App.Contracts.WebRequests;

public class AttendanceCheckRequest
{
    public required bool IsOffline { get; set; }
    public required string StudentCode { get; set; }
    public required string FullName { get; set; }
    public required string AttendanceIdentifier { get; set; }
    public string? WorkplaceIdentifier { get; set; }
}