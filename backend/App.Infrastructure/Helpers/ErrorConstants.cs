namespace App.Infrastructure.Helpers;

public static class ErrorConstants
{
    public const string InvalidCredentials = "invalid-credentials";
    
    public const string SchoolNotFound = "school-not-found";
    public const string SchoolsNotFound = "schools-not-found";
    
    public const string UserTypeNotFound = "usertype-not-found";
    public const string UserTypesNotSeeded = "user-types-not-seeded";
    public const string UsersNotFound = "users-not-found";
    public const string UserNotFound = "user-not-found";
    public const string UserNotDeleted = "user-not-deleted";
    public const string UserNotRestored = "user-not-restored";
    public const string UserNotUpdated = "user-not-updated";
    
    public const string AdminUserNotSeeded = "admin-user-not-seeded";
    
    // Attendance errors
    public const string AttendanceNotFound = "attendance-not-found";
    public const string AttendancesNotFound = "attendances-not-found";
    public const string AttendanceNotCreated = "attendance-not-created";
    public const string AttendanceNotUpdated = "attendance-not-updated";
    public const string AttendanceNotDeleted = "attendance-not-deleted";
    public const string AttendanceNotAccessible = "attendance-not-accessible";
    public const string AttendanceTypeNotFound = "attendance-type-not-found";
    public const string AttendanceTypesNotFound = "attendance-types-not-found";
    public const string AttendanceTypesNotSeeded = "attendance-types-not-seeded";
    public const string StudentsCountNotFound = "students-count-not-found";
    
    // Attendance Check errors
    public const string AttendanceCheckNotFound = "attendance-check-not-found";
    public const string AttendanceChecksNotFound = "attendance-checks-not-found";
    public const string AttendanceCheckNotCreated = "attendance-check-not-created";
    public const string AttendanceCheckNotDeleted = "attendance-check-not-deleted";
    public const string AttendanceCheckNotAccessible = "attendance-check-not-accessible";
    public const string WorkplaceNotFound = "workplace-not-found";
    
    // Course errors
    public const string CourseNotFound = "course-not-found";
    public const string CoursesNotFound = "courses-not-found";
    public const string CourseNotCreated = "course-not-created";
    public const string CourseNotUpdated = "course-not-updated";
    public const string CourseNotDeleted = "course-not-deleted";
    public const string CourseAlreadyExists = "course-already-exists";
    public const string CourseStatusesNotFound = "course-statuses-not-found";
    public const string CourseStatusesNotSeeded = "course-statuses-not-seeded";
    public const string CourseStudentCountsNotFound = "course-student-counts-not-found";
    public const string CourseTeacherNotCreated = "course-teacher-not-created";
    
    // Auth errors
    public const string AuthenticationFailed = "authentication-failed";
    public const string UserNotVerified = "user-not-verified";
    public const string RegistrationFailed = "registration-failed";
    public const string EmailAlreadyExists = "email-already-exists";
    public const string UserTypeNotAvailable = "user-type-not-available";
    public const string PasswordChangeFailed = "password-change-failed";
    public const string InvalidPassword = "invalid-password";
    public const string LogoutFailed = "logout-failed";
    public const string TokenRefreshFailed = "token-refresh-failed";
    
    // OTP errors
    public const string OtpGenerationFailed = "otp-generation-failed";
    public const string OtpVerificationFailed = "otp-verification-failed";
    public const string OtpNotFound = "otp-not-found";
    
    // Refresh Token errors
    public const string RefreshTokenNotGenerated = "refresh-token-not-generated";
    public const string RefreshTokenNotVerified = "refresh-token-not-verified";
    public const string RefreshTokenNotDeleted = "refresh-token-not-deleted";
    public const string RefreshTokenNotFound = "refresh-token-not-found";
}