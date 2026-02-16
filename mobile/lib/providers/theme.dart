import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_riverpod/legacy.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

const _storage = FlutterSecureStorage();
const _themeModeKey = 'app_theme_mode';

/// Provider for managing app theme mode
final themeModeProvider = StateNotifierProvider<ThemeModeNotifier, ThemeMode>((ref) {
  return ThemeModeNotifier();
});

final platformBrightnessProvider = StateProvider<Brightness>((ref) => Brightness.light);

class ThemeModeNotifier extends StateNotifier<ThemeMode> {
  ThemeModeNotifier() : super(ThemeMode.system) {
    _loadThemeMode();
  }

  /// Load saved theme mode from storage
  Future<void> _loadThemeMode() async {
    try {
      final savedThemeMode = await _storage.read(key: _themeModeKey);
      if (savedThemeMode != null) {
        switch (savedThemeMode) {
          case 'light':
            state = ThemeMode.light;
            break;
          case 'dark':
            state = ThemeMode.dark;
            break;
          case 'system':
            state = ThemeMode.system;
            break;
        }
      }
    } catch (e) {
      // If there's an error, keep default theme mode
      debugPrint('Error loading theme mode: $e');
    }
  }

  /// Change theme mode and persist to storage
  Future<void> setThemeMode(ThemeMode mode) async {
    state = mode;
    try {
      String modeString;
      switch (mode) {
        case ThemeMode.light:
          modeString = 'light';
          break;
        case ThemeMode.dark:
          modeString = 'dark';
          break;
        case ThemeMode.system:
          modeString = 'system';
          break;
      }
      await _storage.write(key: _themeModeKey, value: modeString);
    } catch (e) {
      debugPrint('Error saving theme mode: $e');
    }
  }
}

final isDarkThemeProvider = Provider<bool>((ref) {
  final themeMode = ref.watch(themeModeProvider);
  final brightness = ref.watch(platformBrightnessProvider);
  return themeMode == ThemeMode.dark ||
      (themeMode == ThemeMode.system && brightness == Brightness.dark);
});

class AppColors {
  final Color primary;
  final Color secondary;
  final Color background;
  final Color surface;
  final Color error;
  final Color onPrimary;
  final Color onSecondary;
  final Color onBackground;
  final Color onSurface;
  final Color onError;
  final Color textPrimary;
  final Color textSecondary;
  final Color divider;
  final Color success;
  final Color warning;
  final Color info;

  const AppColors({
    required this.primary,
    required this.secondary,
    required this.background,
    required this.surface,
    required this.error,
    required this.onPrimary,
    required this.onSecondary,
    required this.onBackground,
    required this.onSurface,
    required this.onError,
    required this.textPrimary,
    required this.textSecondary,
    required this.divider,
    required this.success,
    required this.warning,
    required this.info,
  });

  factory AppColors.light() => const AppColors(
    primary: Color(0xFF2070DF),
    secondary: Color(0xFF4A90E2),
    background: Color(0xFFF2F2F2),
    surface: Color(0xFFF5F5F5),
    error: Color(0xFFD32F2F),
    onPrimary: Color(0xFFFFFFFF),
    onSecondary: Color(0xFFFFFFFF),
    onBackground: Color(0xFF212121),
    onSurface: Color(0xFF212121),
    onError: Color(0xFFFFFFFF),
    textPrimary: Color(0xFF262626),
    textSecondary: Color(0xFF757575),
    divider: Color(0xFFE0E0E0),
    success: Color(0xFF4CAF50),
    warning: Color(0xFFFFA726),
    info: Color(0xFF29B6F6),
  );

  factory AppColors.dark() => const AppColors(
    primary: Color(0xFF2070DF),
    secondary: Color(0xFF64B5F6),
    background: Color(0xFF262626),
    surface: Color(0xFF1E1E1E),
    error: Color(0xFFEF5350),
    onPrimary: Color(0xFF000000),
    onSecondary: Color(0xFF000000),
    onBackground: Color(0xFFE0E0E0),
    onSurface: Color(0xFFE0E0E0),
    onError: Color(0xFF000000),
    textPrimary: Color(0xFFF2F2F2),
    textSecondary: Color(0xFFB0B0B0),
    divider: Color(0xFF424242),
    success: Color(0xFF66BB6A),
    warning: Color(0xFFFFB74D),
    info: Color(0xFF4FC3F7),
  );
}

final appColorsProvider = Provider<AppColors>((ref) {
  final isDark = ref.watch(isDarkThemeProvider);
  return isDark ? AppColors.dark() : AppColors.light();
});

ThemeData _buildTheme(AppColors colors, Brightness brightness) {
  return ThemeData(
    useMaterial3: true,
    brightness: brightness,
    colorScheme: ColorScheme(
      brightness: brightness,
      primary: colors.primary,
      secondary: colors.secondary,
      surface: colors.surface,
      error: colors.error,
      onPrimary: colors.onPrimary,
      onSecondary: colors.onSecondary,
      onSurface: colors.onSurface,
      onError: colors.onError,
      onPrimaryContainer: colors.onPrimary,
      onSecondaryContainer: colors.onSecondary,
      onErrorContainer: colors.onError,
      onSurfaceVariant: colors.onSurface,
      outline: colors.divider,
      primaryContainer: colors.primary,
      secondaryContainer: colors.secondary,
      errorContainer: colors.error,
      surfaceContainerHighest: colors.surface,
    ),
    scaffoldBackgroundColor: colors.background,
    dividerColor: colors.divider,
    textTheme: TextTheme(
      displayLarge: TextStyle(color: colors.textPrimary),
      displayMedium: TextStyle(color: colors.textPrimary),
      displaySmall: TextStyle(color: colors.textPrimary),
      headlineLarge: TextStyle(color: colors.textPrimary),
      headlineMedium: TextStyle(color: colors.textPrimary),
      headlineSmall: TextStyle(color: colors.textPrimary),
      titleLarge: TextStyle(color: colors.textPrimary),
      titleMedium: TextStyle(color: colors.textPrimary),
      titleSmall: TextStyle(color: colors.textPrimary),
      bodyLarge: TextStyle(color: colors.textPrimary),
      bodyMedium: TextStyle(color: colors.textPrimary),
      bodySmall: TextStyle(color: colors.textSecondary),
      labelLarge: TextStyle(color: colors.textPrimary),
      labelMedium: TextStyle(color: colors.textPrimary),
      labelSmall: TextStyle(color: colors.textSecondary),
    ),
  );
}

final lightThemeProvider = Provider<ThemeData>(
      (ref) => _buildTheme(AppColors.light(), Brightness.light),
);

final darkThemeProvider = Provider<ThemeData>(
      (ref) => _buildTheme(AppColors.dark(), Brightness.dark),
);