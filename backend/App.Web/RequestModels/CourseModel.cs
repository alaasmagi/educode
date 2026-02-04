namespace App.Web.RequestModels;

public class CourseModel : BaseModel
{
    public Guid? Id { get; set; }
    public required string CourseName { get; set; }
    public required string CourseCode { get; set; }
    public required Guid CourseStatusId { get; set; }
}