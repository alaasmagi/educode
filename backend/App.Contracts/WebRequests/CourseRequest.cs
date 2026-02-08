namespace App.Contracts.WebRequests;

public class CourseRequest : BaseRequest
{
    public Guid? Id { get; set; }
    public required string CourseName { get; set; }
    public required string CourseCode { get; set; }
    public required Guid CourseStatusId { get; set; }
}