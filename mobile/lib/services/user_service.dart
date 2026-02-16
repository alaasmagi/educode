import 'dart:convert';
import 'package:mobile/models/Responses/api_error.dart';
import 'package:mobile/models/result.dart';
import 'package:mobile/services/api/api_client.dart';
import 'package:mobile/services/secure_store.dart';

import '../models/Responses/tokens.dart';
import '../models/Responses/user_dto.dart';

class UserService {
  final ApiClient _apiClient;
  final SecureStore _secureStore;

  UserService(this._apiClient, this._secureStore);


  Future<ApiResponse<UserDto>> getUserById(String userId) async {
    var tokensJson = await _secureStore.get('user_tokens');
    if (tokensJson == null) {
      return Failure(ApiError(message: 'No tokens included', code: "no-tokens"));
    }

    var tokens = Tokens.fromJson(jsonDecode(tokensJson));

    final response = await _apiClient.get(
        'User/$userId',
        headers: {
          'Authorization': 'Bearer ${tokens.accessToken}'
        }
    );

    if (response.statusCode == 200) {
      var result = Success(UserDto.fromJson(response.data));
      await _secureStore.delete('user_data');
      await _secureStore.save('user_data', jsonEncode(result.data.toJson()));
      return result;
    } else {
      return Failure(ApiError.fromJson(response.data));
    }
  }
}

