/// Login Provider
///
/// See provider kontrollib, kas kasutaja on secure storage'is olemas
/// ja pakub juurdepääsu kasutaja objektile.
///
/// Kasutamine:
///
/// 1. Kontrolli, kas kasutaja on sisse logitud:
/// ```dart
/// final isLoggedIn = ref.watch(isLoggedInProvider);
/// if (isLoggedIn) {
///   // Kasutaja on sisse logitud
/// }
/// ```
///
/// 2. Kasutaja objekti lugemine:
/// ```dart
/// final user = ref.watch(currentUserProvider);
/// if (user != null) {
///   print('Email: ${user.email}');
/// }
/// ```
///
/// 3. Kasutaja salvestamine:
/// ```dart
/// final loginStateNotifier = ref.read(loginStateProvider.notifier);
/// await loginStateNotifier.setUser(userDto);
/// ```
///
/// 4. Kasutaja välja logimine:
/// ```dart
/// final loginStateNotifier = ref.read(loginStateProvider.notifier);
/// await loginStateNotifier.clearUser();
/// ```
///
/// 5. Kasutaja andmete värskendamine:
/// ```dart
/// final loginStateNotifier = ref.read(loginStateProvider.notifier);
/// await loginStateNotifier.refreshUser();
/// ```

import 'dart:convert';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import 'package:mobile/models/Responses/user_dto.dart';
import 'package:mobile/providers/api_providers.dart';

part 'login_provider.g.dart';

@riverpod
class LoginState extends _$LoginState {
  @override
  Future<UserDto?> build() async {
    return await _loadUserFromStorage();
  }

  Future<UserDto?> _loadUserFromStorage() async {
    try {
      final secureStore = ref.read(secureStoreProvider);
      final userDataJson = await secureStore.get('user_data');

      if (userDataJson != null && userDataJson.isNotEmpty) {
        final userMap = jsonDecode(userDataJson) as Map<String, dynamic>;
        return UserDto.fromJson(userMap);
      }
      return null;
    } catch (e) {
      return null;
    }
  }

  Future<void> setUser(UserDto user) async {
    try {
      final secureStore = ref.read(secureStoreProvider);
      await secureStore.save('user_data', jsonEncode(user.toJson()));
      state = AsyncValue.data(user);
    } catch (e) {
      state = AsyncValue.error(e, StackTrace.current);
    }
  }

  Future<void> clearUser() async {
    try {
      final secureStore = ref.read(secureStoreProvider);
      await secureStore.delete('user_data');
      if (ref.mounted) {
        state = const AsyncValue.data(null);
      }
    } catch (e) {
      if (ref.mounted) {
        state = AsyncValue.error(e, StackTrace.current);
      }
    }
  }

  Future<void> refreshUser() async {
    state = const AsyncValue.loading();
    final user = await _loadUserFromStorage();
    state = AsyncValue.data(user);
  }
}

@riverpod
bool isLoggedIn(Ref ref) {
  final loginState = ref.watch(loginStateProvider);
  return loginState.when(
    data: (user) => user != null,
    loading: () => false,
    error: (_, __) => false,
  );
}

@riverpod
UserDto? currentUser(Ref ref) {
  final loginState = ref.watch(loginStateProvider);
  return loginState.when(
    data: (user) => user,
    loading: () => null,
    error: (_, __) => null,
  );
}
