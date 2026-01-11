# educode-mobile (Under active development)
## Description

* UI language: Estonian or English 
* Development year: **2025**
* Languages and technologies: **TypeScript, React Native, Expo**
* This is the mobile app component of my Bachelor's final thesis project, which also includes [backend](https://github.com/alaasmagi/educode-backend) and [browser client](https://github.com/alaasmagi/educode-web)
* Detailed documentation of my Bachelor's final thesis project (in Estonian):<link>

## How to run

### Prerequisites

* Node.js
* npx package manager
* Expo
* Android device

The application should have .env file in the root folder `/` and it shoult have following content:
```bash
EXPO_PUBLIC_API_URL=<your-educode-backend-instance-url>/api
EXPO_PUBLIC_EMAILDOMAIN=<email-domain-for-otp> //For example: "@taltech.ee"
```

### Running the app

After meeting all prerequisites above - 
* browser client application can be run via terminal/cmd opened in the root folder `/` by command
```bash
npm i; npx expo start --clear
```
* The mobile app can be launched on your Android device by scanning QR code from terminal/cmd with [the Expo Go app on Google Play](https://play.google.com/store/apps/details?id=host.exp.exponent)

## Features
- Students can sign up and log in with university email addresses
- Students can enter the application's offline mode without logging in
- Teachers can log in with university email addresses
- Students can register to course attendances in online mode
- Students can be registered to course attendances in offline mode
- Teachers can register offline mode students in course attendances
- Teachers can manually register students to course attendances

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
There are 10 DTOs in `/models` folder which are responsible for communication between the client app and backend API.
* **AttendanceCheckData**
```typescript
interface AttendanceCheckData {
  studentCode: string;
  courseAttendanceId: number;
  workplaceId?: number;
}

export default AttendanceCheckData;
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
  courseCode: string;
  courseName: string;
  attendanceId?: number;
  attendanceTypeId: string;
  attendanceType: string;
  date: string;
  startTime: string;
  endTime: string;
}
```
* **CreateAttendanceCheckModel**
```typescript
interface CreateAttendanceCheckModel {
  studentCode: string;
  fullName: string;
  courseAttendanceId: number;
  workplaceId?: number;
}

export default CreateAttendanceCheckModel;
```
* **CreateUserModel**
```typescript
interface CreateUserModel {
  uniId: string;
  studentCode: string;
  fullName: string;
  password: string;
}

export default CreateUserModel;
```
* **LocalTokenModel**
```typescript
interface LocalTokenModel {
  uniId: string;
  token: string;
}

export default LocalTokenModel;
```
* **LocalUserData**
```typescript
interface LocalUserData {
  userType: string;
  fullName: string;
  uniId?: string;
  studentCode?: string;
  offlineOnly: boolean;
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
  attendaceCheckId: number;
  studentCode: string;
  workplaceId?: number;
}

export default StudentAttendanceModel;
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
* Student's main views:  
![1](https://github.com/user-attachments/assets/58bbbdd5-c4d3-48fd-8894-c0b30c4387a6)

* Teacher's main views:  
![2](https://github.com/user-attachments/assets/77a8d5d3-37e5-44b6-8b65-94d48316a4d3)


## Improvements & scaling possibilities
### IOS Release
* User testing results suggested the necessity of releasing the app to IOS platform as well
### Integration with more education related services
* User testing results suggested the idea of integrating this application with the existing infrastructure of the University (e.g., the TalTech app)

