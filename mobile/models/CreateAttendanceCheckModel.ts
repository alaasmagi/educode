interface CreateAttendanceCheckModel {
  studentCode: string;
  fullName: string;
  courseAttendanceId: number;
  workplaceId?: number;
}

export default CreateAttendanceCheckModel;
