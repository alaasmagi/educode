class OtpRequest {
  final String clientApp;
  final String email;
  final String fullName;

  OtpRequest({
    required this.clientApp,
    required this.email,
    required this.fullName,
  });

  Map<String, dynamic> toJson() {
    return {
      'clientApp': clientApp,
      'email': email,
      'fullName': fullName,
    };
  }
}

class VerifyOtpRequest {
  final String clientApp;
  final String email;
  final String otp;

  VerifyOtpRequest({
    required this.clientApp,
    required this.email,
    required this.otp,
  });

  Map<String, dynamic> toJson() {
    return {
      'clientApp': clientApp,
      'email': email,
      'otp': otp,
    };
  }
}