# API Teenuste Dokumentatsioon

## Ülevaade

Selles projektis on loodud struktuurne viis API-ga suhtlemiseks, kasutades `dio` paketti HTTP päringute jaoks ja `flutter_dotenv` paketti keskkonnamuutujate haldamiseks.

## Struktuur

```
lib/
├── services/
│   ├── api_client.dart              # Base API klient kõikide HTTP päringute jaoks
│   ├── auth_service.dart            # Auth-spetsiifiline teenus
│   ├── auth_service_example.dart    # Auth kasutamise näited
│   └── general_service.dart         # Üldised teenused (HealthCheck jne)
├── models/
│   └── auth_models.dart             # Auth request/response mudelid
├── providers/
│   ├── api_providers.dart           # API teenuste providerid
│   └── api_providers_example.dart   # Provider kasutamise näited
└── main.dart                        # .env laadimine
```

## Seadistamine

### 1. Paigaldamine

Käivita terminal'is:

```bash
flutter pub get
```

### 2. Keskkonnamuutujad

Muuda `.env` failis oma API base URL:

```env
API_BASE_URL=https://your-api-url.com
```

**NB!** Ära lisa `.env` faili git'i! See on juba `.gitignore` failis.

## Kasutamine

### Basic Setup

```dart
import 'package:mobile/services/api_client.dart';
import 'package:mobile/services/auth_service.dart';

// Loo API klient
final apiClient = ApiClient();

// Loo service
final authService = AuthService(apiClient);
```

### 1. Login

```dart
final request = LoginRequest(
  email: 'user@example.com',
  password: 'password123',
);

try {
  final response = await authService.login(request);
  
  // Salvesta token
  apiClient.setAuthToken(response.accessToken);
  
  print('Welcome ${response.user.name}!');
} on ApiException catch (e) {
  print('Error: ${e.message}');
}
```

### 2. Register

```dart
final request = RegisterRequest(
  email: 'new@example.com',
  password: 'secure123',
  name: 'John Doe',
);

final response = await authService.register('invite_token', request);
apiClient.setAuthToken(response.accessToken);
```

### 3. Refresh Token

```dart
final request = RefreshTokenRequest(refreshToken: storedRefreshToken);
final response = await authService.refreshToken(request);
apiClient.setAuthToken(response.accessToken);
```

### 4. Change Password

```dart
final request = ChangePasswordRequest(
  oldPassword: 'old123',
  newPassword: 'new456',
);

await authService.changePassword(request);
```

### 5. Logout

```dart
await authService.logout();
apiClient.clearAuthToken();
```

## Uue Teenuse Loomine

Kui soovid luua uue teenuse (näiteks `UserService`):

### 1. Loo mudel (`lib/models/user_models.dart`)

```dart
class UpdateProfileRequest {
  final String name;
  final String email;

  UpdateProfileRequest({required this.name, required this.email});

  Map<String, dynamic> toJson() {
    return {'name': name, 'email': email};
  }
}

class UserProfile {
  final String id;
  final String name;
  final String email;

  UserProfile({required this.id, required this.name, required this.email});

  factory UserProfile.fromJson(Map<String, dynamic> json) {
    return UserProfile(
      id: json['id'],
      name: json['name'],
      email: json['email'],
    );
  }
}
```

### 2. Loo teenus (`lib/services/user_service.dart`)

```dart
import 'package:mobile/services/api_client.dart';
import 'package:mobile/models/user_models.dart';

class UserService {
  final ApiClient _apiClient;

  UserService(this._apiClient);

  // GET
  Future<UserProfile> getProfile() async {
    final response = await _apiClient.get('/api/User/Profile');
    
    if (response.statusCode == 200 && response.data != null) {
      return UserProfile.fromJson(response.data);
    }
    throw ApiException('Profiili laadimine ebaõnnestus', response.statusCode);
  }

  // PUT
  Future<UserProfile> updateProfile(UpdateProfileRequest request) async {
    final response = await _apiClient.put(
      '/api/User/Profile',
      data: request.toJson(),
    );
    
    if (response.statusCode == 200 && response.data != null) {
      return UserProfile.fromJson(response.data);
    }
    throw ApiException('Profiili uuendamine ebaõnnestus', response.statusCode);
  }

  // DELETE
  Future<void> deleteAccount() async {
    final response = await _apiClient.delete('/api/User/Account');
    
    if (response.statusCode != 200) {
      throw ApiException('Konto kustutamine ebaõnnestus', response.statusCode);
    }
  }
}
```

## HTTP Meetodid

`ApiClient` toetab kõiki peamisi HTTP meetodeid:

- **GET** - Andmete lugemine
- **POST** - Uute andmete loomine
- **PUT** - Täielik andmete uuendamine
- **PATCH** - Osaline andmete uuendamine
- **DELETE** - Andmete kustutamine

### Näited

```dart
// GET with query parameters
final response = await apiClient.get(
  '/api/users',
  queryParameters: {'page': 1, 'limit': 10},
);

// POST with body
final response = await apiClient.post(
  '/api/users',
  data: {'name': 'John', 'email': 'john@example.com'},
);

// PUT
final response = await apiClient.put(
  '/api/users/123',
  data: {'name': 'John Updated'},
);

// PATCH
final response = await apiClient.patch(
  '/api/users/123',
  data: {'email': 'newemail@example.com'},
);

// DELETE
final response = await apiClient.delete('/api/users/123');
```

## Riverpod Integratsioon

Loo providerid (`lib/providers/api_providers.dart`):

```dart
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:mobile/services/api_client.dart';
import 'package:mobile/services/auth_service.dart';

// API Client
final apiClientProvider = Provider<ApiClient>((ref) {
  return ApiClient();
});

// Auth Service
final authServiceProvider = Provider<AuthService>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  return AuthService(apiClient);
});
```

Kasuta widgetis:

```dart
class LoginScreen extends ConsumerWidget {
  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final authService = ref.watch(authServiceProvider);
    
    return ElevatedButton(
      onPressed: () async {
        try {
          final response = await authService.login(loginRequest);
          // Handle success
        } on ApiException catch (e) {
          // Handle error
        }
      },
      child: Text('Login'),
    );
  }
}
```

## Errorite Käsitlemine

API klient käsitleb automaatselt erinevaid vigu:

```dart
try {
  await authService.login(request);
} on ApiException catch (e) {
  // API spetsiifiline viga
  print('API Error: ${e.message}');
  print('Status Code: ${e.statusCode}');
} catch (e) {
  // Muu viga
  print('Unexpected error: $e');
}
```

## Token Management

```dart
// Seadista token
apiClient.setAuthToken('your_access_token');

// Eemalda token
apiClient.clearAuthToken();

// Token lisatakse automaatselt Authorization headerisse:
// Authorization: Bearer your_access_token
```

## Debug Mode

API klient logib automaatselt kõik päringud ja vastused consolesse debug režiimis. Vaata `api_client.dart` interceptoreid.

## Best Practices

1. **Kasuta mudeleid** - Ära saada/võta raw Map'e, kasuta alati mudeleid
2. **Käsitle vigu** - Kasuta `try-catch` blokke ja näita kasutajale sõnumeid
3. **Salvesta tokeneid turvaliselt** - Kasuta `flutter_secure_storage`
4. **Värskenda tokenit automaatselt** - Implementeeri token refresh logic
5. **Kasuta Riverpod'i** - Loo providerid teenuste jaoks

## Lisainfo

Vaata detailseid kasutamise näiteid failist `lib/services/auth_service_example.dart`.

