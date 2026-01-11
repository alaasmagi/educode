interface AttendanceCheckModel {
  id?: string;
  studentCode: string;
  fullName: string;
  courseAttendanceId: string;
  workplaceId?: string | null;
}

export default AttendanceCheckModel;
