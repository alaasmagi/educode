export interface CourseAttendance {
  courseId: string;
  courseCode?: string;
  courseName?: string;
  attendanceId?: string;
  attendanceTypeId: string;
  attendanceType?: string;
  date: string;
  startTime: string;
  endTime: string;
}

export interface MultipleCourseAttendances {
  attendanceId?: string;
  courseId: string;
  attendanceTypeId: string;
  dates: string[];
  startTime: string;
  endTime: string;
}
