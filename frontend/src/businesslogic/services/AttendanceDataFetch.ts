import { GetJwtToken } from "./UserDataOffline";
import axios from "axios";
import { CourseAttendance, MultipleCourseAttendances } from "../../models/CourseAttendanceModel";
import {
  ConvertDateTimeToDateOnly,
  ConvertDateTimeToTimeOnly,
  ConvertLocalTimeToUtcTimeOnly,
  ConvertUTCToLocalDateTime,
} from "../helpers/DateHandlers";
import AttendanceType from "../../models/AttendanceTypeModel";
import AttendanceCheckData from "../../models/AttendanceCheckModel";
import AttendanceCheckModel from "../../models/AttendanceCheckModel";

export async function AddAttendanceCheck(model: AttendanceCheckModel): Promise<boolean | string> {
  const token = await GetJwtToken();
  const response = await axios.post(
    `${import.meta.env.VITE_API_URL}/Attendance/AttendanceCheck/Add`,
    {
      studentCode: model.studentCode,
      fullName: model.fullName,
      courseAttendanceId: model.courseAttendanceId,
      workplaceId: model.workplaceId,
      creator: "educode-webapp",
    },
    {
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      validateStatus: (status) => status <= 500,
    }
  );
  if (response.status === 200 && !response.data.messageCode) {
    return true;
  }
  return response.data.messageCode ?? "internet-connection-error";
}

export async function GetCurrentAttendance(uniId: string): Promise<CourseAttendance | string> {
  const token = await GetJwtToken();
  const response = await axios.get(`${import.meta.env.VITE_API_URL}/Attendance/CurrentAttendance/UniId/${uniId}`, {
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    withCredentials: true,
    validateStatus: (status) => status <= 500,
  });

  if (response.status === 200 && !response.data.messageCode) {
    const data = response.data;
    return {
      courseId: data.course.id,
      courseCode: data.course.courseCode,
      courseName: data.course.courseName,
      attendanceId: data.id,
      attendanceTypeId: data.attendanceTypeId,
      attendanceType: data.attendanceType.attendanceType,
      date: ConvertDateTimeToDateOnly(data.endTime),
      startTime: ConvertDateTimeToTimeOnly(data.startTime),
      endTime: ConvertDateTimeToTimeOnly(data.endTime),
    } as CourseAttendance;
  }

  return response.data.messageCode ?? "internet-connection-error";
}

export async function GetStudentCountByAttendanceId(attendanceId: number): Promise<number | string> {
  const token = await GetJwtToken();
  const response = await axios.get(
    `${import.meta.env.VITE_API_URL}/Attendance/StudentCount/AttendanceId/${attendanceId}`,
    {
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      withCredentials: true,
      validateStatus: (status) => status <= 500,
    }
  );

  if (response.status === 200 && !response.data.messageCode) {
    const data = response.data;
    return data;
  }

  return response.data.messageCode ?? "internet-connection-error";
}

export async function GetMostRecentAttendance(uniId: string): Promise<CourseAttendance | string> {
  const token = await GetJwtToken();
  const response = await axios.get(`${import.meta.env.VITE_API_URL}/Attendance/RecentAttendance/UniId/${uniId}`, {
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    withCredentials: true,
    validateStatus: (status) => status <= 500,
  });

  if (response.status === 200 && !response.data.messageCode) {
    const data = response.data;
    return {
      courseId: data.course.id,
      courseCode: data.course.courseCode,
      courseName: data.course.courseName,
      attendanceId: data.id,
      attendanceTypeId: data.attendanceTypeId,
      attendanceType: data.attendanceType.attendanceType,
      date: ConvertDateTimeToDateOnly(data.endTime),
      startTime: ConvertDateTimeToTimeOnly(data.startTime),
      endTime: ConvertDateTimeToTimeOnly(data.endTime),
    } as CourseAttendance;
  }

  return response.data.messageCode ?? "internet-connection-error";
}

export async function GetAttendanceById(attendanceId: number): Promise<CourseAttendance | string> {
  const token = await GetJwtToken();
  const response = await axios.get(`${import.meta.env.VITE_API_URL}/Attendance/Id/${attendanceId}`, {
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    withCredentials: true,
    validateStatus: (status) => status <= 500,
  });
  if (response.status === 200 && !response.data.messageCode) {
    const data = response.data;
    return {
      courseId: data.course.id,
      courseCode: data.course.courseCode,
      courseName: data.course.courseName,
      attendanceId: data.id,
      attendanceTypeId: data.attendanceTypeId,
      attendanceType: data.attendanceType.attendanceType,
      date: ConvertDateTimeToDateOnly(data.startTime),
      startTime: ConvertDateTimeToTimeOnly(data.startTime),
      endTime: ConvertDateTimeToTimeOnly(data.endTime),
    } as CourseAttendance;
  }

  return response.data.messageCode ?? "internet-connection-error";
}

export async function GetAttendanceTypes(): Promise<AttendanceType[] | string> {
  const token = await GetJwtToken();
  const response = await axios.get(`${import.meta.env.VITE_API_URL}/Attendance/AttendanceTypes`, {
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    withCredentials: true,
    validateStatus: (status) => status <= 500,
  });
  if (response.status === 200 && !response.data.messageCode) {
    const data = response.data;
    return data;
  }

  return response.data.messageCode ?? "internet-connection-error";
}

export async function AddAttendances(attendances: MultipleCourseAttendances): Promise<boolean | string> {
  const token = await GetJwtToken();
  const response = await axios.post(
    `${import.meta.env.VITE_API_URL}/Attendance/Add`,
    {
      courseId: attendances.courseId,
      attendanceTypeId: attendances.attendanceTypeId,
      startTime: ConvertLocalTimeToUtcTimeOnly(attendances.startTime),
      endTime: ConvertLocalTimeToUtcTimeOnly(attendances.endTime),
      attendanceDates: attendances.dates,
      creator: "educode-webapp",
    },
    {
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      validateStatus: (status) => status <= 500,
    }
  );
  if (response.status === 200 && !response.data.messageCode) {
    return true;
  }

  return response.data.messageCode ?? "internet-connection-error";
}

export async function GetAttendancesByCourseCode(courseCode: string): Promise<CourseAttendance[] | string> {
  const token = await GetJwtToken();
  const response = await axios.get(`${import.meta.env.VITE_API_URL}/Attendance/CourseCode/${courseCode}`, {
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    validateStatus: (status) => status <= 500,
  });
  if (response.status === 200 && !response.data.messageCode) {
    const courseAttendances: CourseAttendance[] = response.data.map((item: any) => ({
      courseId: item.courseId,
      courseCode: item.course.courseCode,
      courseName: item.course.courseName,
      attendanceId: item.id,
      attendanceTypeId: item.attendanceTypeId,
      attendanceType: item.attendanceType,
      date: ConvertUTCToLocalDateTime(item.startTime),
      startTime: ConvertDateTimeToTimeOnly(item.startTime),
      endTime: ConvertDateTimeToTimeOnly(item.endTime),
    }));

    return courseAttendances;
  }
  return response.data.messageCode ?? "internet-connection-error";
}

export async function DeleteAttendance(attendanceId: number): Promise<boolean | string> {
  const token = await GetJwtToken();
  const response = await axios.delete(`${import.meta.env.VITE_API_URL}/Attendance/Delete/${attendanceId}`, {
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    validateStatus: (status) => status <= 500,
  });
  if (response.status === 200 && !response.data.messageCode) {
    return true;
  }

  return response.data.messageCode ?? "internet-connection-error";
}

export async function DeleteAttendanceCheck(attendanceCheckId: number): Promise<boolean | string> {
  const token = await GetJwtToken();
  const response = await axios.delete(
    `${import.meta.env.VITE_API_URL}/Attendance/AttendanceCheck/Delete/${attendanceCheckId}`,
    {
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      validateStatus: (status) => status <= 500,
    }
  );
  if (response.status === 200 && !response.data.messageCode) {
    return true;
  }

  return response.data.messageCode ?? "internet-connection-error";
}

export async function GetAttendanceChecksByAttendanceId(attendanceId: number): Promise<AttendanceCheckData[] | string> {
  const token = await GetJwtToken();
  const response = await axios.get(
    `${import.meta.env.VITE_API_URL}/Attendance/AttendanceChecks/AttendanceId/${attendanceId}`,
    {
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      withCredentials: true,
      validateStatus: (status) => status <= 500,
    }
  );
  if (response.status === 200 && !response.data.messageCode) {
    const data = response.data;
    return data;
  }

  return response.data.messageCode ?? "internet-connection-error";
}
