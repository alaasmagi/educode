import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:mobile/models/Requests/create_account.dart';
import 'package:mobile/models/Requests/otp.dart';
import 'package:mobile/models/Responses/school_dto.dart';
import 'package:mobile/models/result.dart';
import 'package:mobile/providers/api_providers.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';

part 'create_account_controller.g.dart';

@riverpod
class CreateAccountController extends _$CreateAccountController {
  String _fullName = '';
  String _schoolId = '';
  String _schoolDomain = '';
  String _emailLocalPart = '';
  String _studentCode = '';
  String _password = '';
  String _otp = '';

  @override
  void build() {
    ref.keepAlive();
  }

  String get fullName => _fullName;
  String get schoolId => _schoolId;
  String get schoolDomain => _schoolDomain;
  String get emailLocalPart => _emailLocalPart;
  String get studentCode => _studentCode;
  String get password => _password;
  String get otp => _otp;

  String get fullEmail => '$_emailLocalPart@$_schoolDomain';

  void setFullName(String value) {
    _fullName = value;
  }

  void setSchool(SchoolDto school) {
    _schoolId = school.id;
    _schoolDomain = school.domain;
  }

  void setEmailLocalPart(String value) {
    _emailLocalPart = value;
  }

  void setStudentCode(String value) {
    _studentCode = value;
  }

  void setPassword(String value) {
    _password = value;
  }

  void setOtp(String value) {
    _otp = value;
  }

  Future<ApiResponse<List<SchoolDto>>> fetchSchools() async {
    final schoolService = ref.read(schoolServiceProvider);
    return await schoolService.getAllSchools();
  }

  Future<ApiResponse> submitRegistration() async {
    final authService = ref.read(authServiceProvider);

    final request = CreateAccountRequest(
      clientApp: 'educode-mobile',
      fullName: _fullName,
      email: fullEmail,
      password: _password,
      schoolId: _schoolId,
      studentCode: _studentCode,
    );

    return await authService.register(request);
  }

  Future<ApiResponse> requestOtp() async {
    final authService = ref.read(authServiceProvider);

    final request = OtpRequest(
      clientApp: 'educode-mobile',
      email: fullEmail,
      fullName: _fullName,
    );

    return await authService.requestOtp(request);
  }

  Future<ApiResponse> verifyOtp() async {
    final authService = ref.read(authServiceProvider);

    final request = VerifyOtpRequest(
      clientApp: 'educode-mobile',
      email: fullEmail,
      otp: _otp,
    );

    return await authService.verifyOtp(request);
  }

  void reset() {
    _fullName = '';
    _schoolId = '';
    _schoolDomain = '';
    _emailLocalPart = '';
    _studentCode = '';
    _password = '';
    _otp = '';
  }
}

