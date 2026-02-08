namespace App.Contracts.WebRequests;

public class CourseRequest
{ 
    public required string CourseName { get; set; }
    public required string CourseCode { get; set; }
    public required Guid CourseStatusId { get; set; }
}