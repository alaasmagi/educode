# educode - UNDER DEVELOPMENT
## Description

* UI languages: Estonian and English
* Development year: **2025-2026**
* Languages and technologies: **C#, .NET Core, TypeScript, React, Dart, Flutter, Entity Framework Core, JWT, PostgreSQL, Redis**
* This is a unified monorepo containing all three components of my Bachelor's final thesis project: backend API, web application, and mobile application
* Detailed documentation of my Bachelor's final thesis project (in Estonian): [Link to TalTech](https://digikogu.taltech.ee/et/item/6ceef6c1-93b9-428d-a366-8e67b557e207)

**Note**: This repository was previously split into three separate repositories ([backend](https://github.com/alaasmagi/educode-backend), [web client](https://github.com/alaasmagi/educode-web), and [mobile app](https://github.com/alaasmagi/educode-mobile)) and has been unified into a monorepo structure for better maintainability and deployment coordination.

## Version History

### Version 1
This project started as my Bachelor's Thesis project. After graduation I have been improving this project alot. For clarity, I have separated the documentation of old and new version of the project. For the documentation of the old version of the project see the [LEGACY README](LEGACY.md)

### Version 2 (Current)
This is the current implementation (v2) of the educode platform with enhanced features, improved architecture, and a Flutter-based mobile application.

## Project status
* **Database:** implemented and set up
* **Backend:** mostly implemented
* **Mobile:** currently implementing
* **Frontend:** implementation planned

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
* Languages and technologies: **Dart, Flutter**
* Mobile app for students to register attendance via QR codes with offline mode support
* For detailed information, see [the mobile README](./mobile/README.md)

## How to run

### Prerequisites

* **Backend**: Docker, .NET 10.0 SDK
* **Frontend**: Node.js (v18+), modern web browser
* **Mobile**: Flutter SDK (3.0+), Dart SDK, Android Studio or VS Code with Flutter extensions, Android device or emulator

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
# Configure environment settings (see mobile/README.md for configuration details)
flutter pub get
flutter run
```
The mobile app can be launched on your Android device or emulator. Use `flutter devices` to see available devices.

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

## Architecture & Design

### System Architecture

![System Architecture](https://github.com/user-attachments/assets/81759b02-9164-412e-bde8-17e84710bbe8)

*The system architecture diagram will show the overall design schema including the relationships between the backend API, web application, mobile application, database, cache layer, and external services.*

### Entity Relationship Diagram (ERD)

![Entity Relationship Diagram](https://github.com/user-attachments/assets/c25bc4db-39a2-4bac-af95-0906e285621b)

*The ERD shows the database schema including all entities, their attributes, and relationships between tables.*

## Repository Structure

```
educode/
├── backend/                    # .NET Core API server
│   ├── App.Application/        # Application services and business logic
│   ├── App.Contracts/          # Service interfaces, DTOs, and contracts
│   │   ├── DTOs/               # Data Transfer Objects
│   │   ├── Repositories/       # Repository interfaces
│   │   ├── Services/           # Service interfaces
│   │   ├── WebRequests/        # Request models
│   │   └── WebResponse/        # Response models
│   ├── App.Domain/             # Domain entities and enums
│   │   ├── Entities/           # Domain entities
│   │   └── Enums/              # Enumerations
│   ├── App.Infrastructure/     # Infrastructure layer
│   │   ├── Argon2/             # Password hashing
│   │   ├── EFCore/             # Entity Framework Core implementation
│   │   ├── JWT/                # JWT authentication
│   │   ├── Migrations/         # Database migrations
│   │   ├── Oracle/             # Oracle database support
│   │   ├── Redis/              # Redis caching
│   │   └── Sentry/             # Error tracking
│   ├── App.Web/                # ASP.NET Core Web API
│   │   ├── ApiControllers/     # API endpoints
│   │   ├── Controllers/        # MVC controllers
│   │   ├── ViewModels/         # View models
│   │   ├── Views/              # Razor views
│   │   └── wwwroot/            # Static files
│   ├── Base.Domain/            # Base domain classes and error handling
│   ├── Base.DTO/               # Base DTOs and responses
│   ├── Tests/                  # Test projects
│   │   ├── App.BLL Tests/      # Business logic tests
│   │   ├── Bruno/              # API testing collections
│   │   ├── DAL Tests/          # Data access tests
│   │   └── WebApp Tests/       # Web application tests
│   ├── WebApp/                 # Legacy web application
│   ├── compose.yaml            # Docker Compose configuration
│   └── Dockerfile              # Container configuration
├── frontend/                   # React web application
│   ├── public/                 # Static assets
│   │   └── assets/             # Images and resources
│   ├── services/               # API services and i18n
│   └── src/                    # Source code
│       ├── assets/             # Icons and logos
│       ├── businesslogic/      # Core logic and state management
│       ├── layout/             # UI components and layouts
│       ├── locales/            # Translation files (i18n)
│       ├── models/             # TypeScript models and DTOs
│       └── screens/            # Page components
├── mobile/                     # Flutter mobile application
│   ├── android/                # Android platform files
│   ├── ios/                    # iOS platform files
│   ├── assets/                 # Icons, images, and resources
│   ├── lib/                    # Dart source code
│   │   ├── businesslogic/      # State management and logic
│   │   ├── layout/             # UI components
│   │   ├── locales/            # Translation files (i18n)
│   │   ├── models/             # Data models
│   │   └── screens/            # Screen components
│   └── test/                   # Unit and widget tests
├── functional-requirements.md  # Functional requirements document
├── LICENSE                     # Project license
└── README.md                   # This file
```

## Security

- JWT-based authentication and authorization
- Argon2 password encryption
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

## License

This project is licensed under the terms specified in the LICENSE file.
