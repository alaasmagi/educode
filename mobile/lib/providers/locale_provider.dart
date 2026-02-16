import 'package:flutter/material.dart';
import 'package:flutter_riverpod/legacy.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

const _storage = FlutterSecureStorage();
const _localeKey = 'app_locale';

/// Provider for managing app locale
final localeProvider = StateNotifierProvider<LocaleNotifier, Locale>((ref) {
  return LocaleNotifier();
});

class LocaleNotifier extends StateNotifier<Locale> {
  LocaleNotifier() : super(const Locale('et')) {
    _loadLocale();
  }

  /// Load saved locale from storage
  Future<void> _loadLocale() async {
    try {
      final savedLocale = await _storage.read(key: _localeKey);
      if (savedLocale != null) {
        state = Locale(savedLocale);
      }
    } catch (e) {
      // If there's an error, keep default locale
      debugPrint('Error loading locale: $e');
    }
  }

  /// Change locale and persist to storage
  Future<void> setLocale(Locale locale) async {
    state = locale;
    try {
      await _storage.write(key: _localeKey, value: locale.languageCode);
    } catch (e) {
      debugPrint('Error saving locale: $e');
    }
  }

  /// Toggle between Estonian and English
  Future<void> toggleLocale() async {
    final newLocale = state.languageCode == 'et'
        ? const Locale('en')
        : const Locale('et');
    await setLocale(newLocale);
  }
}

