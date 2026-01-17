namespace WebApp.RequestModels.User;

public class CurrentLectureReturnModel
{
    public string CourseName { get; set; } = default!;
    public string CourseCode { get; set; } = default!;
    public Guid AttendanceId { get; set; }
}