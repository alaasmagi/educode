# educode Mobile App (v2)

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
* **Local Auth**: Biometric authentication support

---

## Features

### For Students
- **Account Management**
  - Sign up with university email
  - Email verification via OTP
  - Secure login with biometric support
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
- **Offline Support** (planned)
  - Cache attendance data
  - Sync when connection restored

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

## Project Structure

### Views (`/lib/views/`)
UI screens and pages:
- **auth_gate_view.dart** - Authentication gateway (redirects based on login state)
- **login_view.dart** - Login screen
- **create_account_view.dart** - Registration screen
- **otp_verification_view.dart** - OTP verification
- **role_selection_view.dart** - Role selection for multi-role users
- **student_home_view.dart** - Student dashboard
- **teacher_home_view.dart** - Teacher dashboard

### Controllers (`/lib/controllers/`)
Business logic and state management:
- **login_controller.dart** - Login flow management
- **create_account_controller.dart** - Registration flow
- **otp_controller.dart** - OTP verification logic
- **student_home_view_controller.dart** - Student home logic

### Providers (`/lib/providers/`)
Riverpod state providers:
- **login_provider.dart** - Authentication state
- **api_providers.dart** - API service instances
- **loading_provider.dart** - Global loading state
- **locale_provider.dart** - Language/locale state
- **theme.dart** - Theme configuration

### Services (`/lib/services/`)
API communication and external services:
- **auth_service.dart** - Authentication API calls
- **user_service.dart** - User management API
- **school_service.dart** - School/institution API
- **general_service.dart** - General utility APIs
- **secure_store.dart** - Encrypted local storage
- **loading_manager.dart** - Loading state management
- **api/** - API client configuration and interceptors

### Models (`/lib/models/`)
Data models with Freezed:
- **Requests/** - API request models
- **Responses/** - API response models
- **result.dart** - Result type for error handling

### Widgets (`/lib/widgets/`)
Reusable UI components:
- **form_text_field.dart** - Custom text input
- **email_with_domain_field.dart** - Email input with domain
- **normal_button.dart** - Standard button
- **link_button.dart** - Link-style button
- **default_checkbox.dart** - Custom checkbox
- **school_dropdown.dart** - School selection dropdown
- **language_switcher.dart** - Language toggle
- **theme_toggle.dart** - Dark/Light mode toggle
- **loading_overlay.dart** - Loading indicator overlay
- **camera_preview.dart** - Camera preview for QR scanning
- **section_divider.dart** - UI section divider
- **app_logo.dart** - Application logo widget

### Localization (`/lib/l10n/`)
Internationalization files:
- **app_et.arb** - Estonian translations (default)
- **app_en.arb** - English translations
- Generated localization classes

---

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

## Development Workflow

### Code Generation

When modifying Riverpod providers or Freezed models, regenerate code:

```bash
# Watch mode (auto-regenerates on changes)
flutter pub run build_runner watch

# One-time generation
flutter pub run build_runner build --delete-conflicting-outputs
```

### Adding Translations

1. Add entries to `lib/l10n/app_et.arb` (Estonian - template)
2. Add corresponding entries to `lib/l10n/app_en.arb` (English)
3. Run code generation: `flutter gen-l10n` (or `flutter pub get`)
4. Use in code: `AppLocalizations.of(context)!.yourKey`

### Adding New Routes

1. Define route in `lib/router.dart`:
```dart
GoRoute(
  path: '/new-screen',
  builder: (context, state) => NewScreenView(),
)
```

2. Navigate to route:
```dart
context.go('/new-screen');  // Replace current route
context.push('/new-screen'); // Push on stack
```

---

## Testing

### Unit Tests

```bash
flutter test
```

### Widget Tests

```bash
flutter test test/widget_test.dart
```

### Integration Tests

```bash
flutter test integration_test/
```

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

## Contributing

For information on contributing to the educode project, see the main repository [README](../README.md).

---

## License

See the [LICENSE](./LICENSE) file in the mobile directory.

---

## Version History

- **v2 (Current)**: Complete rewrite in Flutter with improved architecture, Riverpod state management, and enhanced security
- **v1**: Initial React Native/Expo implementation - see [LEGACY.md](./LEGACY.md) for details
