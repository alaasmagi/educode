# educode
## Description

* UI languages: Estonian and English (depending on component)
* Development year: **2025**
* Languages and technologies: **C#, .NET Core, TypeScript, React, React Native, Entity Framework Core, JWT, PostgreSQL, Redis**
* This is a unified monorepo containing all three components of my Bachelor's final thesis project: backend API, web application, and mobile application
* Detailed documentation of my Bachelor's final thesis project (in Estonian):<link>

**Note**: This repository was previously split into three separate repositories ([backend](https://github.com/alaasmagi/educode-backend), [web client](https://github.com/alaasmagi/educode-web), and [mobile app](https://github.com/alaasmagi/educode-mobile)) and has been unified into a monorepo structure for better maintainability and deployment coordination.

## Components

This monorepo contains three main components:

### Backend API
* Languages and technologies: **C#, .NET Core, ASP.NET MVC, Entity Framework Core, JWT, PostgreSQL, Redis**
* Provides RESTful API for authentication, course management, attendance tracking, and data persistence
* For detailed information, see [the backend README](./backend/README.md)

### Web Application
* Languages and technologies: **TypeScript, React, Vite, Tailwind CSS**
* Browser client for teachers to manage courses, attendance sessions, and view reports
* For detailed information, see [the frontend README](./frontend/README.md)

### Mobile Application
* Languages and technologies: **TypeScript, React Native, Expo**
* Mobile app for students to register attendance via QR codes with offline mode support
* For detailed information, see [the mobile README](./mobile/README.md)

## How to run

### Prerequisites

* **Backend**: Docker, .NET 8.0 SDK
* **Frontend**: Node.js (v18+), modern web browser
* **Mobile**: Node.js (v18+), npx package manager, Expo, Android device

### Running the components

#### Backend
```bash
cd backend
# Configure .env file (see backend/README.md for environment variables)
docker-compose up
```
The backend API will be available at the configured host and port.

#### Web Application
```bash
cd frontend
# Configure .env file (see frontend/README.md for environment variables)
npm install
npm start
```
The web UI can be viewed from the web browser on the address provided in the terminal.

#### Mobile Application
```bash
cd mobile
# Configure .env file (see mobile/README.md for environment variables)
npm install
npx expo start --clear
```
The mobile app can be launched on your Android device by scanning QR code from terminal with the Expo Go app.

For detailed setup instructions including environment variables and configuration options, refer to each component's README.

## Features

### Backend
- JWT-based authentication and authorization
- Email verification with OTP for account creation and password recovery
- Role-based access control (teachers, students, administrators)
- Course management API
- Attendance tracking and registration
- PostgreSQL for data persistence
- Redis for caching and session management
- RESTful API architecture

### Web Application (Teachers)
- Teachers can sign up and log in with university email addresses
- Teachers can manage courses
- Teachers can manage course attendances
- Teachers can view QR codes for each course attendance so students can register themselves
- Teachers can manually register students to course attendances
- Teachers can view the list of registered students for each course attendance
- Teachers can download the list of registered students as a PDF
- Teachers can view statistics of course attendances by course
- Multilingual support (Estonian/English)

### Mobile Application (Students)
- Students can sign up and log in with university email addresses
- Students can enter the application's offline mode without logging in
- Teachers can log in with university email addresses
- Students can register to course attendances in online mode
- Students can be registered to course attendances in offline mode
- Teachers can register offline mode students in course attendances
- Teachers can manually register students to course attendances
- QR code scanning for quick attendance check-in
- Multilingual support (Estonian/English)

## Repository Structure

```
educode/
├── backend/          # .NET Core API server
│   ├── App.BLL/      # Business Logic Layer
│   ├── App.Contracts/# Service interfaces
│   ├── App.DAL.EF/   # Data Access Layer (Entity Framework)
│   ├── App.Domain/   # Domain entities
│   ├── App.DTO/      # Data Transfer Objects
│   ├── Base.Domain/  # Base domain classes
│   ├── Tests/        # Unit and integration tests
│   └── WebApp/       # ASP.NET Core Web API
├── frontend/         # React web application
│   ├── public/       # Static assets
│   ├── services/     # API services
│   └── src/          # Source code
│       ├── assets/   # Icons and logos
│       ├── businesslogic/  # Core logic
│       ├── layout/   # UI components
│       ├── locales/  # Translations
│       └── models/   # DTOs
├── mobile/           # React Native mobile app
│   ├── app/          # App screens and navigation
│   ├── assets/       # Icons and logos
│   ├── businesslogic/# Core logic
│   ├── layout/       # UI components
│   ├── locales/      # Translations
│   ├── modals/       # Modal components
│   ├── models/       # DTOs
│   └── screens/      # Screen components
├── LICENSE           # Project license
└── README.md         # This file
```

## Security

- JWT-based authentication and authorization
- BCrypt password hashing (12 rounds) with Base64 encoding for admin credentials
- OTP-based email verification for account creation and password recovery
- Environment-based configuration for sensitive data
- CORS protection
- Secure token generation and validation

## Design choices

### Monorepo Structure
The project was restructured from three separate repositories into a unified monorepo to:
- Simplify dependency management across components
- Enable atomic commits affecting multiple parts of the system
- Improve coordination between frontend, backend, and mobile development
- Streamline deployment and version control
- Facilitate code sharing and refactoring

### Technology Stack
- **Backend**: .NET Core was chosen for its performance, scalability, and strong typing
- **Web**: React with Vite provides fast development experience and optimal production builds
- **Mobile**: React Native with Expo enables cross-platform development with a single codebase
- **Database**: PostgreSQL for reliable data persistence with strong ACID guarantees
- **Cache**: Redis for session management and performance optimization

## License

This project is licensed under the terms specified in the LICENSE file.
