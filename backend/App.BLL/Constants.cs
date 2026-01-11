namespace App.BLL;

public class Constants
{
    public const int DefaultQueryPageSize = 20;
    
    public const string UserIdClaim = "UserId";
    public const string AccessLevelClaim = "AccessLevel";
    public const string RefreshTokenPrefix = "RefreshToken:";
    public const string OtpPrefix = "OTP:";
    public const string UserPrefix = "User:";
    public const string UserAuthPrefix = "UserAuth:";
    public const string CoursePrefix = "Course:";
    public const string CurrentAttendancePrefix = "CurrentAttendance:";
    public const string RecentAttendancePrefix = "RecentAttendance:";
    public const string AttendancePrefix = "Attendance:";
    public const string AttendanceAccessPrefix = "AttendanceAccess:";
    public const string AttendanceCheckAccessPrefix = "AttendanceCheckAccess:";
    public const string SchoolPrefix = "School:";
    public const string StudentCountPrefix = "StudentCount:";
    public const string AttendanceCheckPrefix = "AttendanceCheck:";
    public const string AttendanceTypePrefix = "AttendanceType:";
    public const string CourseStatusPrefix = "CourseStatus:";
    public const string CourseStudentCountsPrefix = "StudentCounts:";
    public const string CourseAccessPrefix = "CourseAccess:";
    public const string UserTypePrefix = "UserType:";
    public const string WorkplacePrefix = "Workplace:";
    public const string UserFolder = "user";
    public const string SchoolFolder = "school";
    public const string BackendPrefix = "EDUCODE-ASPNET";
    public const int MaxPictureFileSize = 5 * 1024 * 1024; 

    
    public static readonly TimeSpan DefaultCachePeriod = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan ExtraShortCachePeriod = TimeSpan.FromMinutes(1);
    public static readonly TimeSpan ShortCachePeriod = TimeSpan.FromMinutes(2);
    public static readonly TimeSpan MediumCachePeriod = TimeSpan.FromHours(1);
    public static readonly TimeSpan LongCachePeriod = TimeSpan.FromHours(2);
    public static readonly TimeSpan ExtraLongCachePeriod = TimeSpan.FromHours(12);
}