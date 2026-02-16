# educode Backend (v2)

## Description

* **Development Year**: 2025-2026
* **Languages & Technologies**: C#, .NET 10.0, ASP.NET Core, Entity Framework Core, PostgreSQL, Redis, JWT
* **Architecture**: Clean Architecture with Domain-Driven Design principles
* This is the backend API component of the educode platform, providing RESTful endpoints for the web and mobile clients
* Part of a unified monorepo: see [main README](../README.md) for overall project information
* For legacy v1 documentation, see [LEGACY.md](./LEGACY.md)

## Architecture Overview

The backend follows Clean Architecture principles with clear separation of concerns across multiple layers:

```
App.Web              → Presentation layer (API Controllers, MVC Views)
App.Application      → Application services and business logic
App.Contracts        → Interfaces, DTOs, and contracts
App.Domain           → Domain entities and business rules
App.Infrastructure   → Infrastructure implementations (EF Core, Redis, JWT, etc.)
Base.Domain          → Base entities and shared domain logic
Base.DTO             → Base DTOs and response models
```

### Key Technologies & Infrastructure

* **Database**: PostgreSQL with Entity Framework Core (code-first approach)
* **Caching**: Redis for session management and performance optimization
* **Authentication**: JWT with refresh tokens and secure cookie handling
* **Password Hashing**: Argon2 (OWASP recommended)
* **Error Tracking**: Sentry integration for production monitoring
* **File Storage**: Oracle Cloud Infrastructure (OCI) Object Storage
* **Logging**: Serilog with console and file output
* **Rate Limiting**: Built-in request rate limiting
* **API Documentation**: Swagger/OpenAPI

---

## How to Run

### Prerequisites

* Docker and Docker Compose (recommended)
* OR .NET 10.0 SDK
* PostgreSQL database
* Redis instance

### Option 1: Using Docker Compose (Recommended)

1. Create a `.env` file in the backend directory with the required environment variables (see Environment Variables section below)

2. Run with Docker Compose:
```bash
cd backend
docker compose up --build
```

The API will be available at `http://localhost:8080`

### Option 2: Using .NET SDK

1. Ensure you have .NET 8.0 SDK installed
2. Create a `.env` file in the backend root directory
3. Run the application:

```bash
cd backend/App.Web
dotnet restore
dotnet run
```

### Environment Variables

Create a `.env` file with the following configuration:

```bash
# Database Configuration
PG_DB_CONNECTION=Host=localhost;Port=5432;Database=educode;Username=postgres;Password=yourpassword

# Redis Configuration
REDIS_CONNECTION=localhost:6379

# JWT Configuration
JWTKEY=your-secret-jwt-key-min-32-chars
JWTAUD=educode-audience
JWTISS=educode-issuer
JWT_MINUTES=60
JWT_ADMIN_MINUTES=120
JWT_COOKIE_MINUTES=60

# Refresh Token Configuration
REFRESH_TOKEN_DAYS=7
REFRESH_TOKEN_COOKIE_DAYS=7

# OTP Configuration
OTPKEY=your-otp-secret-key
OTP_MINUTES=10

# Admin Configuration
DEFAULT_ADMIN_USER=admin@educode.ee
DEFAULT_ADMIN_PASSWORD=YourSecurePassword123!

# Email Service (External API)
EMAIL_API_URL=https://your-email-service/api/send
EMAIL_API_KEY=your-email-api-key
EMAIL_EXPIRY_MINUTES=10

# Sentry (Error Tracking)
SENTRY_DSN=your-sentry-dsn-here

# Oracle Cloud Infrastructure (Optional - for file storage)
OCI_KEY=your-oci-private-key
OCI_TENANCY_ID=your-tenancy-id
OCI_USER_ID=your-user-id
OCI_FINGERPRINT=your-key-fingerprint
OCI_REGION=us-ashburn-1
OCI_BUCKET_NAME=your-bucket-name
OCI_PUBLIC_URL=https://objectstorage.region.oraclecloud.com

# Soft Delete Configuration
SOFTDELETE_EXPIRATION_DAYS=180

# CORS Configuration
FRONTENDURLS=http://localhost:3000;http://localhost:5173
```

---

---

## Features

### Core Functionality
- RESTful API for web and mobile clients
- JWT-based authentication with refresh token rotation
- Role-based authorization (Admin, Teacher, Student)
- Email-based OTP verification for account creation and password recovery
- Course management (CRUD operations)
- Attendance session management
- Student attendance tracking and registration
- School/institution management
- User invitation system
- Comprehensive audit logging with client tracking

### Admin UI
- Built with ASP.NET MVC and Bootstrap
- Database entity management through web interface
- User management and role assignment
- Course and attendance oversight
- System configuration and monitoring

### API Endpoints

The API is organized into the following controller groups:

* **AuthController** - Authentication, login, logout, token refresh
* **UserController** - User management, profile operations
* **CourseController** - Course CRUD operations, teacher assignments
* **AttendanceController** - Attendance session management
* **AttendanceCheckController** - Student attendance registration
* **SchoolController** - School/institution management
* **OtpController** - OTP generation and verification
* **GeneralController** - Health checks and general utilities

---

## Project Structure

```
backend/
├── App.Application/          # Application services layer
│   └── Services/             # Business logic implementations
│       ├── Attendance/       # Attendance management services
│       ├── AttendanceCheck/  # Attendance check services
│       ├── AttendanceType/   # Attendance type services
│       ├── Course/           # Course management services
│       ├── School/           # School management services
│       ├── User/             # User management services
│       └── UserType/         # User type services
│
├── App.Contracts/            # Interfaces and contracts
│   ├── DTOs/                 # Data Transfer Objects
│   ├── Repositories/         # Repository interfaces
│   ├── Services/             # Service interfaces
│   ├── WebRequests/          # API request models
│   └── WebResponse/          # API response models
│
├── App.Domain/               # Domain layer
│   ├── Entities/             # Domain entities
│   │   ├── AttendanceCheckEntity.cs
│   │   ├── AttendanceEntity.cs
│   │   ├── AttendanceTypeEntity.cs
│   │   ├── ClassroomEntity.cs
│   │   ├── CourseEntity.cs
│   │   ├── CourseStatusEntity.cs
│   │   ├── CourseTeacherEntity.cs
│   │   ├── InvitationEntity.cs
│   │   ├── RefreshTokenEntity.cs
│   │   ├── SchoolEntity.cs
│   │   ├── UserAuthEntity.cs
│   │   ├── UserEntity.cs
│   │   ├── UserTypeEntity.cs
│   │   └── WorkplaceEntity.cs
│   └── Enums/                # Domain enumerations
│
├── App.Infrastructure/       # Infrastructure implementations
│   ├── Argon2/               # Argon2 password hashing
│   ├── EFCore/               # Entity Framework Core setup
│   ├── Helpers/              # Utility helpers
│   ├── Initializers/         # Application initializers
│   ├── JSON/                 # JSON serialization
│   ├── JWT/                  # JWT token handling
│   ├── Migrations/           # Database migrations
│   ├── Oracle/               # OCI Object Storage integration
│   ├── Redis/                # Redis caching implementation
│   └── Sentry/               # Sentry error tracking
│
├── App.Web/                  # Presentation layer
│   ├── ApiControllers/       # REST API controllers
│   ├── Clients/              # External service clients
│   ├── Controllers/          # MVC controllers (Admin UI)
│   ├── ViewModels/           # View models for MVC
│   ├── Views/                # Razor views (Admin UI)
│   ├── wwwroot/              # Static files
│   └── Program.cs            # Application startup
│
├── Base.Domain/              # Base domain classes
│   ├── BaseEntity.cs         # Base entity with audit fields
│   └── Error.cs              # Error model
│
├── Base.DTO/                 # Base DTOs
│   └── MethodResponse.cs     # Standard API response wrapper
│
├── Tests/                    # Test projects
│   ├── App.BLL Tests/        # Business logic tests
│   ├── Bruno/                # API testing collections
│   ├── DAL Tests/            # Data access tests
│   └── WebApp Tests/         # Integration tests
│
├── compose.yaml              # Docker Compose configuration
├── Dockerfile                # Container definition
└── educode-backend.sln       # Solution file
```

---

## Architecture & Design Decisions

### Clean Architecture
The application follows Clean Architecture principles to ensure:
- **Separation of Concerns**: Each layer has a specific responsibility
- **Dependency Inversion**: Dependencies point inward toward the domain
- **Testability**: Business logic is isolated and easily testable
- **Maintainability**: Changes in one layer don't cascade to others

### Domain Entities

All entities inherit from `BaseEntity` which provides:
- **Guid-based IDs**: Using GUIDs instead of integers for better distribution and security
- **Audit Tracking**: CreatedBy, CreatedByClient, CreatedAt, UpdatedBy, UpdatedByClient, UpdatedAt
- **Soft Delete**: Logical deletion with `Deleted` flag
- **Validation**: Built-in validation methods

**Key Entities:**

* **UserEntity** - User accounts with university ID and student code
* **UserAuthEntity** - Secure password storage (Argon2 hashed)
* **UserTypeEntity** - Role definitions (Admin, Teacher, Student)
* **SchoolEntity** - Educational institutions
* **CourseEntity** - Course information with status tracking
* **CourseTeacherEntity** - Many-to-many relationship between courses and teachers
* **AttendanceEntity** - Attendance sessions for courses
* **AttendanceTypeEntity** - Types of attendance (lecture, lab, seminar, etc.)
* **AttendanceCheckEntity** - Individual student attendance records
* **WorkplaceEntity** - Physical location tracking (classroom/computer)
* **ClassroomEntity** - Classroom information
* **InvitationEntity** - User invitation system
* **RefreshTokenEntity** - JWT refresh token management
* **CourseStatusEntity** - Course status definitions

### Security Implementation

**Password Hashing:**
- Argon2id algorithm (OWASP recommended, winner of Password Hashing Competition)
- Memory-hard and resistant to GPU attacks
- Configurable memory cost, time cost, and parallelism

**JWT Authentication:**
- Access tokens with configurable expiration
- Refresh token rotation for enhanced security
- Secure HTTP-only cookies for token storage
- Token validation with issuer and audience claims

**OTP System:**
- Time-based one-time passwords for email verification
- Configurable expiration times
- Used for account creation and password recovery

### Performance Optimizations

**Database:**
- Connection pooling (pool size: 128)
- Batch operations (min: 10, max: 128)
- Query result caching
- No-tracking queries for read operations
- Automatic retry on failure (3 attempts)

**Redis Caching:**
- Session data caching
- Frequently accessed data caching
- Reduced database load

**Rate Limiting:**
- Request throttling to prevent abuse
- Configurable limits per endpoint

---

## Deployment

### Docker

Build and run with Docker Compose:
```bash
docker compose up --build -d
```

### Production Considerations

1. **Environment Variables**: Use secure secret management (Azure Key Vault, AWS Secrets Manager, etc.)
2. **Database**: Use managed PostgreSQL service with automatic backups
3. **Redis**: Use managed Redis instance with persistence enabled
4. **Sentry**: Configure for production error tracking
5. **Logging**: Ensure logs are persisted and monitored
6. **HTTPS**: Always use HTTPS in production with valid certificates
7. **Rate Limiting**: Adjust limits based on expected traffic
8. **Backup Strategy**: Regular database backups and disaster recovery plan


## Improvements & Future Enhancements

### Planned Features
- [ ] Advanced analytics and reporting dashboard
- [ ] Integration with university student information systems
- [ ] Mobile push notifications
- [ ] Automated attendance reminders
- [ ] QR code expiration and rotation for enhanced security
- [ ] Multi-factor authentication options
- [ ] API versioning for backward compatibility

### Performance Enhancements
- [ ] Implement CQRS pattern for complex queries
- [ ] Add Elasticsearch for advanced search capabilities
- [ ] Implement response caching with cache invalidation
- [ ] Add CDN support for static assets

### Security Enhancements
- [ ] Implement API key authentication for external integrations
- [ ] Add IP whitelisting for admin endpoints
- [ ] Implement security headers (CSP, HSTS, etc.)
- [ ] Add comprehensive audit logging with tamper detection

---

## License

See the [LICENSE](../LICENSE) file in the root directory.

---

## Version History

- **v2 (Current)**: Complete architectural redesign with Clean Architecture, enhanced security, Redis caching, OCI integration
- **v1**: Initial implementation - see [LEGACY.md](./LEGACY.md) for details

