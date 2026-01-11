import axios from "axios";
import Course from "../../models/CourseModel";
import { GetJwtToken } from "./UserDataOffline";
import { CourseStatus } from "../../models/CourseStatus";
import StudentCountModel from "../../models/StudentCountModel";
import { ConvertDateTimeToDateOnly } from "../helpers/DateHandlers";

export async function GetCoursesByUser(uniId: string): Promise<Course[] | string> {
  const token = await GetJwtToken();
  const response = await axios.get(`${import.meta.env.VITE_API_URL}/Course/UniId/${uniId}`, {
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    validateStatus: (status) => status <= 500,
  });
  if (response.status === 200 && !response.data.messageCode) {
    const courses: Course[] = response.data;
    return courses;
  }
  return response.data.messageCode ?? "internet-connection-error";
}

export async function GetCoursebyId(courseId: number): Promise<Course | string> {
  const token = await GetJwtToken();
  const response = await axios.get(`${import.meta.env.VITE_API_URL}/Course/Id/${courseId}`, {
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    validateStatus: (status) => status <= 500,
  });
  if (response.status === 200 && !response.data.messageCode) {
    const course: Course = response.data;
    return course;
  }
  return response.data.messageCode ?? "internet-connection-error";
}

export async function GetCourseStatuses(): Promise<CourseStatus[] | string> {
  const token = await GetJwtToken();
  const response = await axios.get(`${import.meta.env.VITE_API_URL}/Course/Statuses`, {
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    validateStatus: (status) => status <= 500,
  });
  if (response.status === 200 && !response.data.messageCode) {
    const result: CourseStatus[] = response.data;
    return result;
  }
  return response.data.messageCode ?? "internet-connection-error";
}

export async function AddCourse(uniId: string, courseModel: Course): Promise<boolean | string> {
  const token = await GetJwtToken();
  const response = await axios.post(
    `${import.meta.env.VITE_API_URL}/Course/Add`,
    {
      uniId: uniId,
      courseName: courseModel.courseName,
      courseCode: courseModel.courseCode,
      status: courseModel.courseValidStatus,
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

export async function GetCourseStudentCounts(courseId: number): Promise<StudentCountModel[] | string> {
  const token = await GetJwtToken();
  const response = await axios.get(`${import.meta.env.VITE_API_URL}/Course/StudentCounts/${courseId}`, {
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    validateStatus: (status) => status <= 500,
  });
  if (response.status === 200 && !response.data.messageCode) {
    const studentCounts: StudentCountModel[] = response.data.map((item: any) => ({
      attendanceDate: ConvertDateTimeToDateOnly(item.attendanceDate),
      studentCount: item.userCount,
    }));
    return studentCounts;
  }
  return response.data.messageCode ?? "internet-connection-error";
}

export async function EditCourse(courseId: number, uniId: string, course: Course): Promise<boolean | string> {
  const token = await GetJwtToken();
  const response = await axios.patch(
    `${import.meta.env.VITE_API_URL}/Course/Edit`,
    {
      id: courseId,
      uniId: uniId,
      courseName: course.courseName,
      courseCode: course.courseCode,
      status: course.courseValidStatus,
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

export async function DeleteCourse(courseId: number): Promise<boolean | string> {
  const token = await GetJwtToken();
  const response = await axios.delete(`${import.meta.env.VITE_API_URL}/Course/Delete/${courseId}`, {
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
