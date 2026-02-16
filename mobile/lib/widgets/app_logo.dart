import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:mobile/providers/theme.dart';

class AppLogo extends ConsumerWidget {
  final double height;
  const AppLogo({Key? key, this.height = 100}) : super(key: key);

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final isDark = ref.watch(isDarkThemeProvider);
    return Image(
      image: isDark
          ? const AssetImage('assets/logo/white-on-transparent.png')
          : const AssetImage('assets/logo/black-on-transparent.png'),
      height: height,
    );
  }
}

