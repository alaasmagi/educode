import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:mobile/services/api/api_client.dart';
import 'package:mobile/services/api/token_refresh_interceptor.dart';
import 'package:mobile/services/auth_service.dart';
import 'package:mobile/services/general_service.dart';
import 'package:mobile/services/school_service.dart';
import 'package:mobile/services/user_service.dart';

import '../services/secure_store.dart';

final apiClientProvider = Provider<ApiClient>((ref) {
  return ApiClient();
});

final secureStoreProvider = Provider<SecureStore>((ref) {
  return SecureStore();
});

final authServiceProvider = Provider<AuthService>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  final secureStore = ref.watch(secureStoreProvider);
  return AuthService(apiClient, secureStore);
});

final userServiceProvider = Provider<UserService>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  final secureStore = ref.watch(secureStoreProvider);
  return UserService(apiClient, secureStore);
});

final generalServiceProvider = Provider<GeneralService>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  return GeneralService(apiClient);
});

final schoolServiceProvider = Provider<SchoolService>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  return SchoolService(apiClient);
});

/// API Online State Provider
/// Jälgib, kas API on kättesaadav
/// Kasutab StreamProvider't, et perioodiliselt kontrollida API staatust
final isAppOnlineProvider = StreamProvider<bool>((ref) async* {
  final generalService = ref.watch(generalServiceProvider);

  // Esimene kontroll kohe
  yield await generalService.healthCheck();

  // Seejärel kontrolli iga 30 sekundi tagant
  await for (final _ in Stream.periodic(const Duration(seconds: 30))) {
    yield await generalService.healthCheck();
  }
});

/// Alternatiivne versioon FutureProvider'iga
/// Kui soovid ainult ühekordset kontrolli
final isApiOnlineProvider = FutureProvider<bool>((ref) async {
  final generalService = ref.watch(generalServiceProvider);
  return await generalService.healthCheck();
});

/// Manuaalne refreshi provider
/// Kasuta seda, kui tahad käsitsi healthcheck'i teha
final healthCheckProvider = FutureProvider.autoDispose<bool>((ref) async {
  final generalService = ref.watch(generalServiceProvider);
  return await generalService.healthCheck();
});

/// Provider that initializes the token refresh interceptor
/// This should be watched in the app initialization to set up automatic token refresh
final tokenRefreshInterceptorInitProvider = Provider<void>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  final secureStore = ref.watch(secureStoreProvider);

  // Add token refresh interceptor that automatically handles 403 errors
  final interceptor = TokenRefreshInterceptor(
    dio: apiClient.dio,
    secureStore: secureStore,
    onRefreshFailed: () async {
      // When token refresh fails, clear all data
      await secureStore.delete('user_data');
      await secureStore.delete('user_tokens');
    },
  );

  apiClient.addInterceptor(interceptor);
});

