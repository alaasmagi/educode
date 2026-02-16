class CreateAccountRequest {
  final String clientApp;
  final String fullName;
  final String email;
  final String password;
  final String schoolId;
  final String studentCode;

  CreateAccountRequest({
    required this.clientApp,
    required this.fullName,
    required this.email,
    required this.password,
    required this.schoolId,
    required this.studentCode,
  });

  Map<String, dynamic> toJson() {
    return {
      'clientApp': clientApp,
      'fullname': fullName,
      'email': email,
      'password': password,
      'schoolId': schoolId,
      'studentCode': studentCode,
    };
  }
}