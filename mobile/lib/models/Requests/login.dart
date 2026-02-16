class LoginRequest {
  final String email;
  final String password;
  final String clientApp;

  LoginRequest({
    required this.email,
    required this.password,
    required this.clientApp,
  });

  Map<String, dynamic> toJson() {
    return {
      'email': email,
      'password': password,
      'clientApp': clientApp,
    };
  }
}