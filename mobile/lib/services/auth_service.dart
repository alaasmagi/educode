import 'dart:convert';

import 'package:mobile/models/Requests/change_password.dart';
import 'package:mobile/models/Requests/create_account.dart';
import 'package:mobile/models/Requests/login.dart';
import 'package:mobile/models/Requests/otp.dart';
import 'package:mobile/models/Requests/refresh_tokens.dart';
import 'package:mobile/models/Responses/api_error.dart';
import 'package:mobile/models/Responses/login_response.dart';
import 'package:mobile/models/result.dart';
import 'package:mobile/services/api/api_client.dart';
import 'package:mobile/services/secure_store.dart';

import '../models/Responses/tokens.dart';
import '../models/Responses/user_dto.dart';

class AuthService {
  final ApiClient _apiClient;
  final SecureStore _secureStore;
  AuthService(this._apiClient, this._secureStore);


  Future<ApiResponse<LoginResponse>> login(LoginRequest request) async {
    final response = await _apiClient.post('Auth/Login', data: request.toJson());
    if (response.statusCode == 200) {
      var result = Success(LoginResponse.fromJson(response.data));
      await _secureStore.save('user_data', jsonEncode(result.data.user.toJson()));
      await _secureStore.save('user_tokens', jsonEncode(result.data.tokens.toJson()));
      return result;
    } else {
      return Failure(ApiError.fromJson(response.data));
    }
  }

  Future<ApiResponse<UserDto>> register(CreateAccountRequest request) async {
    final response = await _apiClient.post('Auth/Register', data: request.toJson());

    if (response.statusCode == 200) {
      var result = Success(UserDto.fromJson(response.data));
      await _secureStore.save('user_data', jsonEncode(result.data.toJson()));
      return result;
    }

    return Failure(ApiError.fromJson(response.data));
  }

  Future<ApiResponse<Tokens>> refreshToken(RefreshTokensRequest request) async {
    final response = await _apiClient.post('Auth/Refresh', data: request.toJson());

    if (response.statusCode == 200) {
      var result = Success(Tokens.fromJson(response.data));
      await _secureStore.delete('user_tokens');
      await _secureStore.save('user_tokens', jsonEncode(result.data.toJson()));
      return result;
    }

    return Failure(ApiError.fromJson(response.data));
  }

  Future<ApiResponse<bool>> changePassword(ChangePasswordRequest request) async {
    var tokensJson = await _secureStore.get('user_tokens');
    if (tokensJson == null) {
      return Failure(ApiError(message: 'No tokens included', code: "no-tokens"));
    }

    var tokens = Tokens.fromJson(jsonDecode(tokensJson));
    final response = await _apiClient.post(
        'Auth/ChangePassword',
        data: request.toJson(),
        headers: {
          'Authorization': 'Bearer ${tokens.accessToken}'
        }
    );

    if (response.statusCode == 200) {
      return Success(true);
    }

    return Failure(ApiError.fromJson(response.data));
  }

  Future<ApiResponse<bool>> logout() async {
    var tokensJson = await _secureStore.get('user_tokens');
    if (tokensJson == null) {
      return Failure(ApiError(message: 'No tokens included', code: "no-tokens"));
    }

    var tokens = Tokens.fromJson(jsonDecode(tokensJson));
    final response = await _apiClient.post('Auth/Logout', data: { 'refreshToken': tokens.refreshToken, });

    if (response.statusCode == 200) {
      return Success(true);
    }

    return Failure(ApiError.fromJson(response.data));
  }

  Future<ApiResponse<bool>> requestOtp(OtpRequest request) async {
    final response = await _apiClient.post('Otp/Request', data: request.toJson());

    if (response.statusCode == 200) {
      return Success(true);
    }

    return Failure(ApiError.fromJson(response.data));
  }

  Future<ApiResponse<bool>> verifyOtp(VerifyOtpRequest request) async {
    final response = await _apiClient.post('Otp/Verify', data: request.toJson());

    if (response.statusCode == 200) {
      return Success(true);
    }

    return Failure(ApiError.fromJson(response.data));
  }

  Future<void> clearAllData() async {
    await _secureStore.delete('user_data');
    await _secureStore.delete('user_tokens');
  }
}

