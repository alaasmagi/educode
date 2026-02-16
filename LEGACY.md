# educode - Version 1 (Legacy)

## Overview

This document describes the original implementation (Version 1) of the EduCode platform developed in 2025 as part of a Bachelor's final thesis project. The system was initially built as three separate repositories that worked together to provide a complete attendance management solution for educational institutions.

* **UI Languages**: Estonian and English
* **Development Year**: 2025
* **Technology Stack**: 
  - Backend: C#, .NET Core, ASP.NET MVC, Entity Framework Core, JWT, MySQL
  - Web Client: TypeScript, React, Vite, Tailwind CSS
  - Mobile App: TypeScript, React Native, Expo
* **Detailed Documentation**: [TalTech Digital Library](https://digikogu.taltech.ee/et/item/6ceef6c1-93b9-428d-a366-8e67b557e207)

## System Architecture

Version 1 consisted of three separate repositories:
- [educode-backend](https://github.com/alaasmagi/educode-backend) - RESTful API and Admin UI
- [educode-web](https://github.com/alaasmagi/educode-web) - Browser client for teachers
- [educode-mobile](https://github.com/alaasmagi/educode-mobile) - Mobile app for students

These components communicated via REST API endpoints, with JWT-based authentication and MySQL as the data store.

---

## Components

### Backend ([detailed documentation](./backend/LEGACY.md))

* **Technology**: C#, .NET Core 9.0, ASP.NET MVC, Entity Framework Core
* **Key Features**:
  - RESTful API with JWT authentication
  - Admin UI for database management using ASP.NET MVC Views
  - 7 main services (Authentication, Course Management, Attendance Management, Email/OTP, User Management)
  - Code-first database approach with 9 entities
  - 100% unit test coverage using NUnit
  - Automatic cleanup service for old attendances
* **Deployment**: Available as Docker image or via .NET SDK

See [backend/LEGACY.md](./backend/LEGACY.md) for complete details including database schema, entity definitions, service architecture, and setup instructions.

### Frontend ([detailed documentation](./frontend/LEGACY.md))

* **Technology**: TypeScript, React, Vite, Tailwind CSS
* **Key Features**:
  - Teacher-focused web interface
  - Course and attendance management
  - QR code generation for student check-ins
  - Manual student registration
  - Attendance statistics and PDF export
  - Multilingual support (Estonian/English)
* **Project Structure**: 6 main folders (assets, businesslogic, layout, locales, models, screens)

See [frontend/LEGACY.md](./frontend/LEGACY.md) for complete details including all 13 DTOs, setup instructions, and UI screenshots.

### Mobile ([detailed documentation](./mobile/LEGACY.md))

* **Technology**: TypeScript, React Native, Expo
* **Target Platform**: Android devices
* **Key Features**:
  - Student-focused mobile app
  - QR code scanning for attendance
  - Offline mode support
  - Teacher functionality for offline student registration
  - Multilingual support (Estonian/English)
* **Project Structure**: 6 main folders (assets, businesslogic, layout, locales, models, screens)

See [mobile/LEGACY.md](./mobile/LEGACY.md) for complete details including all 10 DTOs, setup instructions, and app screenshots.

---

## Transition to Version 2

The lessons learned from Version 1 led to significant architectural changes in Version 2:

1. **Monorepo Structure**: Unified all three repositories into a single monorepo for better coordination and atomic commits
2. **Mobile Technology**: Migrated from React Native/Expo to Flutter for better performance, native capabilities, and improved developer experience
3. **Enhanced Architecture**: Improved separation of concerns with dedicated Application and Infrastructure layers
4. **Better Deployment**: Streamlined deployment process with comprehensive Docker Compose configuration
5. **Code Sharing**: Enabled easier sharing of types, utilities, and contracts across components
6. **Improved Documentation**: Centralized documentation with component-specific details maintained separately

For details on the current implementation, see the main [README.md](./README.md).
