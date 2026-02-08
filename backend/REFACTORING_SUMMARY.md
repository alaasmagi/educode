# UserService Decomposition Summary

## Overview
The original `UserService` was a "god" service with too many responsibilities. It has been decomposed into three specialized services, with the original `UserService` now acting as a facade for backward compatibility.

## New Service Architecture

### 1. **IUserManagementService** → `UserManagementService`
**Location:** `App.Application/Services/User/UserManagementService.cs`

**Responsibilities:**
- User CRUD operations
- Soft delete and restore operations with cascading effects
- Cache management for user data

**Methods:**
- `GetAllUsersAsync(int pageNr, int pageSize)` - Paginated user retrieval
- `GetUserByIdAsync(Guid id)` - Single user retrieval
- `UpdateUserAsync(UserEntity user)` - User updates
- `SoftDeleteUserAsync(Guid userId)` - Soft delete with cascading to related entities
- `RestoreUserAsync(Guid userId)` - Restore soft-deleted users

**Dependencies:**
- `IUserRepository`
- `ICourseTeacherRepository` (for cascade deletion)
- `IAttendanceCheckRepository` (for cascade deletion)
- `IRefreshTokenRepository` (for cascade deletion)
- `IUserAuthRepository` (for cascade deletion)
- `ICacheRepository`

---

### 2. **IUserTypeService** → `UserTypeService`
**Location:** `App.Application/Services/User/UserTypeService.cs`

**Responsibilities:**
- User type management
- User type seeding
- Cache management for user type data

**Methods:**
- `GetUserTypeAsync(string userType)` - Retrieve user type by name
- `SeedUserTypes()` - Initialize default user types (student, teacher-assistant, teacher, school-administrator, system-administrator)

**Dependencies:**
- `IUserTypeRepository`
- `ICacheRepository`

---

### 3. **ISeedingService** → `SeedingService`
**Location:** `App.Application/Services/User/SeedingService.cs`

**Responsibilities:**
- Database seeding operations
- Admin user initialization

**Methods:**
- `SeedAdminUser()` - Create default admin user from environment variables

**Dependencies:**
- `IUserRepository`
- `IPasswordService`
- `IUserAuthRepository`
- `IUserTypeRepository`
- `EnvInitializer`

---

### 4. **IUserService** → `UserService` (Facade)
**Location:** `App.Application/Services/User/UserService.cs`

**Responsibilities:**
- Maintain backward compatibility with existing controllers
- Delegate all operations to specialized services

**Methods:**
- All methods from the three specialized services
- Simply delegates to the appropriate specialized service

**Dependencies:**
- `IUserManagementService`
- `IUserTypeService`
- `ISeedingService`

---

## Benefits of This Decomposition

### 1. **Single Responsibility Principle**
Each service now has a clear, focused responsibility:
- User management deals with user CRUD
- User type service handles user types
- Seeding service manages initialization

### 2. **Easier Testing**
- Smaller services are easier to unit test
- Mock dependencies are more straightforward
- Test files can be organized by service responsibility

### 3. **Better Maintainability**
- Changes to user type logic don't affect user management
- Seeding logic is isolated and easy to modify
- Each service file is smaller and easier to understand

### 4. **Improved Dependency Management**
- Services only depend on what they need
- Circular dependencies are easier to avoid
- Dependency injection is clearer

### 5. **Backward Compatibility**
- Existing controllers don't need immediate changes
- The facade pattern allows gradual migration
- No breaking changes to the API

---

## Migration Path

### Immediate (Already Done)
✅ Created three specialized service interfaces and implementations
✅ Created facade service for backward compatibility
✅ Registered all services in DI container (`Program.cs`)

### Short Term (Recommended)
Controllers can gradually migrate to use specialized services directly:

**Before:**
```csharp
public class UserController(IUserService userService)
```

**After:**
```csharp
public class UserController(
    IUserManagementService userManagementService,
    IUserTypeService userTypeService)
```

### Long Term (Optional)
Once all controllers are migrated:
- Remove `IUserService` and `UserService` facade
- Clean up documentation references

---

## File Structure

```
App.Contracts/Services/
├── IUserService.cs (facade - kept for backward compatibility)
├── IUserManagementService.cs (new)
├── IUserTypeService.cs (new)
└── ISeedingService.cs (new)

App.Application/Services/User/
├── UserService.cs (facade implementation)
├── UserManagementService.cs (new)
├── UserTypeService.cs (new)
└── SeedingService.cs (new)
```

---

## Dependency Injection Registration

In `Program.cs`:
```csharp
// Register specialized user services
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IUserTypeService, UserTypeService>();
builder.Services.AddScoped<ISeedingService, SeedingService>();
// Facade service for backward compatibility
builder.Services.AddScoped<IUserService, UserService>();
```

---

## Usage Examples

### Using the Facade (Current Controllers)
```csharp
var users = await userService.GetAllUsersAsync(1, 10);
var userType = await userService.GetUserTypeAsync("student");
await userService.SeedUserTypes();
```

### Using Specialized Services (Recommended for New Code)
```csharp
// User management
var users = await userManagementService.GetAllUsersAsync(1, 10);
var user = await userManagementService.GetUserByIdAsync(userId);
await userManagementService.SoftDeleteUserAsync(userId);

// User types
var userType = await userTypeService.GetUserTypeAsync("student");
await userTypeService.SeedUserTypes();

// Seeding
await seedingService.SeedAdminUser();
```

---

## Testing Recommendations

Create separate test files for each service:
- `UserManagementServiceTests.cs`
- `UserTypeServiceTests.cs`
- `SeedingServiceTests.cs`
- `UserServiceTests.cs` (facade - integration tests)

---

## Notes

- **No code logic was changed** - only reorganized into smaller services
- All original functionality is preserved
- Cache invalidation patterns remain the same
- Cascade deletion logic for soft deletes is unchanged
- Error handling and logging patterns are consistent

