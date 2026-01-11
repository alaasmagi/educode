# educode-web
## Description

* UI language: Estonian or English 
* Development year: **2025**
* Languages and technologies: **TypeScript, React, Vite, Tailwind CSS**
* This is the mobile app component of my Bachelor's final thesis project, which also includes [backend](https://github.com/alaasmagi/educode-backend) and [mobile app](https://github.com/alaasmagi/educode-mobile)  
* Detailed documentation of my Bachelor's final thesis project (in Estonian):<link>

## How to run

### Prerequisites

* Node.js
* Modern web browser

The application should have .env file in the root folder `/` and it shoult have following content:
```bash
VITE_API_URL=<your-educode-backend-instance-url>/api
VITE_EMAIL_DOMAIN=<email-domain-for-otp> //For example: "@taltech.ee"
```

### Running the app

After meeting all prerequisites above - 
* browser client application can be run via terminal/cmd opened in the root folder `/` by command
```bash
npm i; npm start
```
* The UI can be viewed from the web browser on the address the application provided in the terminal/cmd

## Features
- Teachers can sign up and log in with university email addresses
- Teachers can manage courses
- Teachers can manage course attendances
- Teachers can view QR codes for each course attendance so students can register themselves
- Teachers can manually register students to course attendances
- Teachers can view the list of registered students for each course attendance
- Teachers can download the list of registered students as a PDF
- Teachers can view statistics of course attendances by course

## Design choices

### Structure
The project is divided into 6 folders:
* **assets** - contains static icons and logos
* **businesslogic** - contains all the core logic of the client application
* **layout** - contains custom-built UI components
* **locales** - contains files for UI translations (localization)
* **models** - contains DTOs used for communication between the client app and the backend API
* **screens** - contains all the views/pages of the browser client application
  
### Data Transfer Objects (DTOs)
There are 13 DTOs in `/models` folder which are responsible for communication between the client app and backend API.
* **AttendanceCheckModel**
```typescript
interface AttendanceCheckModel {
  id?: number;
  studentCode: string;
  fullName: string;
  courseAttendanceId: number;
  workplaceId?: number;
}

export default AttendanceCheckModel;
```
* **AttendanceType**
```typescript
interface AttendanceType {
  id: number;
  attendanceType: string;
}

export default AttendanceType;
```
* **ChangePasswordModel**
```typescript
interface ChangePasswordModel {
    uniId: string;
    newPassword: string;
}

export default ChangePasswordModel;
```
* **CourseAttendance**
```typescript
export interface CourseAttendance {
  courseId: number;
  courseCode?: string;
  courseName?: string;
  attendanceId?: number;
  attendanceTypeId: string;
  attendanceType?: string;
  date: string;
  startTime: string;
  endTime: string;
}

export interface MultipleCourseAttendances {
  attendanceId?: number;
  courseId: number;
  attendanceTypeId: string;
  dates: string[];
  startTime: string;
  endTime: string;
}
```
* **Course**
```typescript
interface Course {
  id?: number;
  courseCode: string;
  courseName: string;
  courseValidStatus: number;
}

export default Course;
```
* **CourseStatus**
```typescript
export interface CourseStatus {
  id: number;
  status: string;
}
```
* **CreateUserModel**
```typescript
interface CreateUserModel {
    uniId: string;
    fullName: string;
    password: string;
}

export default CreateUserModel;
```
* **LocalDataModel**
```typescript
interface LocalDataModel {
    uniId: string;
    token: string;
}

export default LocalDataModel;
```
* **LocalUserData**
```typescript
interface LocalUserData {
    userType: string,
    uniId?: string;
    studentCode?: string;
    offlineOnly: boolean;
    fullName?: string;
}

export default LocalUserData;
```
* **OnlineUserModel**
```typescript
interface OnlineUserModel {
    id: number;
    userType: {
        userType: string;
    };
    uniId: string;
    studentCode?: string;
    fullName: string;
}

export default OnlineUserModel;
```
* **StudentAttendanceModel**
```typescript
interface StudentAttendanceModel {
    attendaceCheckId: number,
    studentCode: string;
    workplaceId?: number;
}

export default StudentAttendanceModel;
```
* **StudentCountModel**
```typescript
interface StudentCountModel {
    attendanceDate: string;
    studentCount: number;
}

export default StudentCountModel;
```
* **VerifyOTPModel**
```typescript
interface VerifyOTPModel {
    uniId: string;
    otp: string;
}

export default VerifyOTPModel;
```

## Screenshots
* Main page view:  
![Screenshot 2025-05-07 143342](https://github.com/user-attachments/assets/597a9494-829a-41a4-bdec-6d2b1f93ead2)
* Attendance QR code view:  
![Screenshot 2025-05-07 143404](https://github.com/user-attachments/assets/f0c51ca4-0641-4eb7-8b22-0f94b2be3bb0)
* Attendances' view:  
![Screenshot 2025-05-07 143421](https://github.com/user-attachments/assets/7cb6f5c0-1c92-460a-a1d6-c2031f68a716)
* Attendance's details:  
![Screenshot 2025-05-07 143354](https://github.com/user-attachments/assets/10feab0a-1020-40fa-86b2-47aa65f8866e)
* Statistics view:  
![Screenshot 2025-05-07 143432](https://github.com/user-attachments/assets/95505dfa-f22c-4c56-b251-773dd9c76918)

## Improvements & scaling possibilities
### Integration with more education related services
* User testing results suggested the idea of integrating this application with the existing infrastructure of the University (e.g., the TalTech app)

