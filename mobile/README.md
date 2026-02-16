# educode Mobile App (v2) - UNDER DEVELOPMENT

## Description

* **Development Year**: 2025-2026
* **Languages & Technologies**: Dart, Flutter
* **Target Platforms**: Android, iOS (planned)
* **Architecture**: MVVM with Riverpod state management
* This is the mobile application component of the educode platform, providing student and teacher attendance management on mobile devices
* Part of a unified monorepo: see [main README](../README.md) for overall project information
* For legacy v1 documentation (React Native), see [LEGACY.md](./LEGACY.md)

## Architecture Overview

The mobile app follows a clean architecture pattern with clear separation of concerns:

```
lib/
├── views/          → UI layer (screens and pages)
├── controllers/    → Business logic and state management
├── providers/      → Riverpod providers (state, API, configuration)
├── services/       → API communication and external services
├── models/         → Data models (requests/responses)
├── widgets/        → Reusable UI components
├── l10n/           → Internationalization (i18n)
├── router.dart     → Navigation configuration
├── constants/      → Application constants
├── enums/          → Enumerations
├── utils/          → Utility functions
└── validators/     → Form validators
```

### Key Technologies & Libraries

* **State Management**: Riverpod (Provider pattern)
* **HTTP Client**: Dio (with interceptors)
* **Navigation**: GoRouter (declarative routing)
* **Local Storage**: Flutter Secure Storage (encrypted)
* **Authentication**: Firebase Auth, Google Sign-In
* **Internationalization**: Flutter Intl (Estonian, English)
* **Code Generation**: Freezed, Riverpod Generator, Build Runner
* **Camera**: Camera package (QR code scanning)
* **Permissions**: Permission Handler
  
---

## Features

### For Students
- **Account Management**
  - Sign up with university email
  - Email verification via OTP
  - Offline attendance registration
  - Password recovery
- **Attendance Registration**
  - QR code scanning for attendance check-in
  - View active attendance sessions
  - Attendance history
- **School Selection**
  - Choose from available schools/institutions
- **User Interface**
  - Dark/Light theme toggle
  - Language switching (Estonian/English)
  - Intuitive material design

### For Teachers
- **Multi-Role Access**
  - Role selection (Student/Teacher toggle)
  - Separate dashboards for each role
- **Attendance Management**
  - View attendance sessions
  - Manual student registration
  - Access to attendance reports
- **Course Overview**
  - View assigned courses
  - Monitor attendance statistics

### For All Users
- **Secure Authentication**
  - JWT token-based authentication
  - Automatic token refresh
  - Secure storage of credentials

---

## How to Run

### Prerequisites

* **Flutter SDK**: 3.10.4 or higher
* **Dart SDK**: ^3.10.4
* **Android Studio** or **VS Code** with Flutter extensions
* **Android Device/Emulator** (iOS support planned)
* Running backend API (see [backend README](../backend/README.md))

### Installation

1. **Clone the repository and navigate to mobile directory**:
```bash
cd mobile
```

2. **Install dependencies**:
```bash
flutter pub get
```

3. **Generate code** (for Riverpod, Freezed):
```bash
flutter pub run build_runner build --delete-conflicting-outputs
```

4. **Configure environment variables**:

Create a `.env` file in the mobile directory:

```dotenv
API_BASE_URL=http://your-backend-url:8080/api
```

5. **Run the app**:

For Android:
```bash
flutter run
```

For specific device:
```bash
flutter devices  # List available devices
flutter run -d <device-id>
```

For release build:
```bash
flutter build apk --release
```

---

## Key Implementation Details

### Secure Storage

User credentials and tokens are stored securely using `flutter_secure_storage`:

```dart
// Store token
await secureStore.write(key: 'access_token', value: token);

// Retrieve token
final token = await secureStore.read(key: 'access_token');
```

### API Communication

Using Dio with custom interceptors for:
- Automatic token injection
- Token refresh on 401 responses
- Request/response logging
- Error handling

### Navigation

GoRouter provides declarative routing with:
- Route guards (authentication checks)
- Deep linking support
- Type-safe navigation
- Route parameters

### Internationalization

Flutter's built-in i18n support with ARB files:
- Dynamic language switching
- Context-aware translations
- Plural and select support

---

## Building for Production

### Android APK

```bash
# Build release APK
flutter build apk --release

# Build App Bundle (for Google Play)
flutter build appbundle --release
```

### Android Release Configuration

Update `android/key.properties` with signing configuration:
```properties
storePassword=<password>
keyPassword=<password>
keyAlias=<key-alias>
storeFile=<path-to-keystore>
```

### iOS (Planned)

```bash
flutter build ios --release
```

---

## Environment Configuration

### Development
- Uses `.env` file for local development
- Debug logging enabled
- Points to local/development backend

### Production
- Environment variables injected at build time
- Debug logging disabled
- Points to production backend API

---

## Documentation

Additional documentation files:
- [LEGACY.md](./LEGACY.md) - Version 1 (React Native) documentation

---

## License

See the [LICENSE](./LICENSE) file in the mobile directory.

---

## Version History

- **v2 (Current)**: Complete rewrite in Flutter with improved architecture, Riverpod state management, and enhanced security
- **v1**: Initial React Native/Expo implementation - see [LEGACY.md](./LEGACY.md) for details
