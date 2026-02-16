class RefreshTokensRequest {
  final String clientApp;
  final String accessToken;
  final String refreshToken;

  RefreshTokensRequest({
    required this.clientApp,
    required this.accessToken,
    required this.refreshToken,
  });

  Map<String, dynamic> toJson() {
    return {
      'clientApp': clientApp,
      'accessToken': accessToken,
      'refreshToken': refreshToken,
    };
  }
}