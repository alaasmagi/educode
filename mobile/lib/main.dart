import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:mobile/providers/api_providers.dart';
import 'package:mobile/providers/locale_provider.dart';
import 'package:mobile/router.dart';
import 'package:mobile/providers/theme.dart';
import 'package:mobile/widgets/loading_overlay.dart';
import 'package:mobile/l10n/app_localizations.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await dotenv.load(fileName: ".env");

  runApp(const ProviderScope(child: MyApp()));
}

class MyApp extends ConsumerWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    ref.watch(tokenRefreshInterceptorInitProvider);

    final themeMode = ref.watch(themeModeProvider);
    final lightTheme = ref.watch(lightThemeProvider);
    final darkTheme = ref.watch(darkThemeProvider);
    final locale = ref.watch(localeProvider);

    return MaterialApp.router(
      title: 'EduCode',
      theme: lightTheme,
      darkTheme: darkTheme,
      themeMode: themeMode,
      locale: locale,
      localizationsDelegates: const [
        AppLocalizations.delegate,
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],
      supportedLocales: const [
        Locale('et'),
        Locale('en'),
      ],
      debugShowCheckedModeBanner: false,
      routerConfig: router,
      builder: (context, child) {
        final brightness = MediaQuery.of(context).platformBrightness;
        WidgetsBinding.instance.addPostFrameCallback((_) {
          ref.read(platformBrightnessProvider.notifier).state = brightness;
        });
        return LoadingOverlay(child: child ?? const SizedBox.shrink());
      },
    );
  }
}
