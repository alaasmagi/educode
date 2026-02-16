import 'package:mobile/models/Responses/tokens.dart';
import 'package:mobile/models/Responses/user_dto.dart';

class LoginResponse {
  final UserDto user;
  final Tokens tokens;

  LoginResponse({
    required this.user,
    required this.tokens,
  });

  factory LoginResponse.fromJson(Map<String, dynamic> json) {
    return LoginResponse(
      user: UserDto.fromJson(json['user']),
      tokens: Tokens(
        accessToken: json['accessToken'] ?? '',
        refreshToken: json['refreshToken'] ?? '',
      ),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'user': user.toJson(),
      'accessToken': tokens.accessToken,
      'refreshToken': tokens.refreshToken,
    };
  }
}
