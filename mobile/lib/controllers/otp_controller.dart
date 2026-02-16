import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import 'package:mobile/models/Requests/otp.dart';
import 'package:mobile/models/result.dart';
import 'package:mobile/providers/api_providers.dart';

part 'otp_controller.g.dart';

@riverpod
class OtpController extends _$OtpController {
  String _email = '';
  String _fullName = '';
  String _otp = '';

  @override
  void build() {
    // Keep the provider alive during async operations
    ref.keepAlive();
  }

  String get email => _email;
  String get fullName => _fullName;
  String get otp => _otp;

  void setEmail(String value) {
    _email = value;
  }

  void setFullName(String value) {
    _fullName = value;
  }

  void setOtp(String value) {
    _otp = value;
  }

  Future<ApiResponse> requestOtp() async {
    final authService = ref.read(authServiceProvider);

    final request = OtpRequest(
      clientApp: 'educode-mobile',
      email: _email,
      fullName: _fullName,
    );

    return await authService.requestOtp(request);
  }

  Future<ApiResponse> verifyOtp() async {
    final authService = ref.read(authServiceProvider);

    final request = VerifyOtpRequest(
      clientApp: 'educode-mobile',
      email: _email,
      otp: _otp,
    );

    return await authService.verifyOtp(request);
  }

  void reset() {
    _email = '';
    _fullName = '';
    _otp = '';
  }
}


