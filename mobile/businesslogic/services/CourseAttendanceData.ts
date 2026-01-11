import CreateAttendanceCheckModel from "../../models/CreateAttendanceCheckModel";
import { CourseAttendance } from "../../models/CourseAttendance";
import { GetUserToken } from "./UserDataOffline";
import Constants from "expo-constants";
import axios from "axios";

export async function AddAttendanceCheck(model: CreateAttendanceCheckModel): Promise<boolean | string> {
  const token = await GetUserToken();
  const response = await axios.post(
    `${Constants.expoConfig?.extra?.EXPO_PUBLIC_API_URL}/Attendance/AttendanceCheck/Add`,
    {
      studentCode: model.studentCode,
      fullName: model.fullName,
      courseAttendanceId: model.courseAttendanceId,
      workplaceId: model.workplaceId,
      creator: "educode-mobile",
    },
    {
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      validateStatus: (status) => status < 500,
    }
  );

  if (response.status == 200 && !response.data.messageCode) {
    return true;
  }
  return response.data.messageCode ?? "internet-connection-error";
}

export async function GetCurrentAttendance(uniId: string): Promise<CourseAttendance | string> {
  const token = await GetUserToken();
  const response = await axios.get(
    `${Constants.expoConfig?.extra?.EXPO_PUBLIC_API_URL}/Attendance/CurrentAttendance/UniId/${uniId}`,
    {
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      validateStatus: (status) => status < 500,
    }
  );

  if (response.status == 200 && !response.data.messageCode) {
    const data = response.data;
    return {
      courseId: data.course.id,
      courseCode: data.course.courseCode,
      courseName: data.course.courseName,
      attendanceId: data.id,
      attendanceTypeId: data.attendanceTypeId,
      attendanceType: data.attendanceType.attendanceType,
      date: data.endTime,
      startTime: data.startTime,
      endTime: data.endTime,
    } as CourseAttendance;
  }
  return response.data.messageCode ?? "internet-connection-error";
}
