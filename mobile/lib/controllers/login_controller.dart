import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:mobile/router.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../models/Requests/login.dart';
import '../models/result.dart';
import '../providers/api_providers.dart';

part 'login_controller.g.dart';

@riverpod
class LoginController extends _$LoginController {
  String _email = '';
  String _password = '';
  String _studentCode = '';
  String _fullName = '';

  @override
  void build() {
    ref.keepAlive();
  }

  String get email => _email;
  String get password => _password;
  String get studentCode => _studentCode;
  String get fullName => _fullName;

  void setEmail(String value) {
    _email = value;
  }

  void setPassword(String value) {
    _password = value;
  }

  void setStudentCode(String value) {
    _studentCode = value;
  }

  void setFullName(String value) {
    _fullName = value;
  }

  Future<ApiResponse> submitLogin() async {
    final authService = ref.read(authServiceProvider);
    final request = LoginRequest(
      email: _email,
      password: _password,
      clientApp: 'educode-mobile',
    );

    final result = await authService.login(request);

    if (!ref.mounted) {
      return result;
    }

    if (result is Success) {
      router.go('/');
    } else if (result is Failure) {
      final failure = result as Failure;
      if (failure.error.code == 'user-not-verified') {
        router.go('/otp-verification?email=${Uri.encodeComponent(_email)}&fullName=&isPostLogin=true');
      }
    }

    return result;
  }

  Future<void> submitOfflineMode() async {
  }

  void reset() {
    _email = '';
    _password = '';
    _studentCode = '';
    _fullName = '';
  }
}
