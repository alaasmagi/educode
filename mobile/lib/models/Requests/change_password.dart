class ChangePasswordRequest {
  final String userId;
  final String clientApp;
  final String email;
  final String currentPassword;
  final String newPassword;

  ChangePasswordRequest({
    required this.userId,
    required this.clientApp,
    required this.email,
    required this.currentPassword,
    required this.newPassword,
  });

  Map<String, dynamic> toJson() {
    return {
      'userId': userId,
      'clientApp': clientApp,
      'email': email,
      'currentPassword': currentPassword,
      'newPassword': newPassword,
    };
  }
}