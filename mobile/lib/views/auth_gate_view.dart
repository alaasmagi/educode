import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:mobile/models/result.dart';
import 'package:mobile/providers/api_providers.dart';
import 'package:mobile/providers/login_provider.dart';

import '../models/Responses/user_dto.dart';

/// AuthGateView - vaheetapp, mis kontrollib kasutaja login state'i,
/// fetchib API-st värske kasutajaandmed ning suunab vastavalt accessLevel'ile õigele lehele
class AuthGateView extends ConsumerStatefulWidget {
  const AuthGateView({Key? key}) : super(key: key);

  @override
  ConsumerState<AuthGateView> createState() => _AuthGateViewState();
}

class _AuthGateViewState extends ConsumerState<AuthGateView> {
  bool _hasNavigated = false;
  bool _isVerifying = false;

  @override
  Widget build(BuildContext context) {
    final loginState = ref.watch(loginStateProvider);

    return loginState.when(
      data: (user) {
        if (_hasNavigated) {
          return const Scaffold(
            body: Center(child: CircularProgressIndicator()),
          );
        }

        if (user == null) {
          WidgetsBinding.instance.addPostFrameCallback((_) {
            _navigateTo('/login');
          });
          return const Scaffold(
            body: Center(child: CircularProgressIndicator()),
          );
        }

        // User exists, verify with API and then navigate
        if (!_isVerifying) {
          _isVerifying = true;
          WidgetsBinding.instance.addPostFrameCallback((_) {
            _verifyUserAndNavigate(user);
          });
        }

        return const Scaffold(
          body: Center(child: CircularProgressIndicator()),
        );
      },
      loading: () => const Scaffold(
        body: Center(child: CircularProgressIndicator()),
      ),
      error: (error, stack) {
        if (!_hasNavigated) {
          WidgetsBinding.instance.addPostFrameCallback((_) async {
            final authService = ref.read(authServiceProvider);
            await authService.clearAllData();
            _navigateTo('/login');
          });
        }

        return const Scaffold(
          body: Center(child: CircularProgressIndicator()),
        );
      },
    );
  }

  Future<void> _verifyUserAndNavigate(UserDto user) async {
    if (_hasNavigated || !mounted) {
      setState(() {
        _isVerifying = false;
      });
      return;
    }

    try {
      // Try to fetch fresh data from API
      final userService = ref.read(userServiceProvider);
      final result = await userService.getUserById(user.id);

      if (!mounted) {
        setState(() {
          _isVerifying = false;
        });
        return;
      }

      if (result is Success<UserDto>) {
        final accessLevel = result.data.accessLevel;
        String route;
        if (accessLevel == 1) {
          route = '/student-home';
        } else if (accessLevel == 2) {
          route = '/role-selection';
        } else if (accessLevel == 3) {
          route = '/teacher-home';
        } else if (accessLevel >= 4) {
          route = '/role-selection';
        } else {
          route = '/login';
        }

        if (mounted) {
          setState(() {
            _hasNavigated = true;
          });
        }

        final loginStateNotifier = ref.read(loginStateProvider.notifier);
        await loginStateNotifier.setUser(result.data);

        if (mounted) {
          context.go(route);
        }
      } else {
        final authService = ref.read(authServiceProvider);
        await authService.clearAllData();

        final loginStateNotifier = ref.read(loginStateProvider.notifier);
        await loginStateNotifier.clearUser();

        _navigateTo('/login');
      }
    } catch (e) {
      // Error during verification, clear and go to login
      if (mounted) {
        final authService = ref.read(authServiceProvider);
        await authService.clearAllData();

        final loginStateNotifier = ref.read(loginStateProvider.notifier);
        await loginStateNotifier.clearUser();

        _navigateTo('/login');
      }
    } finally {
      if (mounted) {
        setState(() {
          _isVerifying = false;
        });
      }
    }
  }


  void _navigateTo(String route) {
    if (!mounted || _hasNavigated) return;

    _hasNavigated = true;
    context.go(route);
  }
}

