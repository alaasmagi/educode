class UserDto {
  final String id;
  final String email;
  final String userTypeId;
  final String userType;
  final int accessLevel;
  final String studentCode;
  final String photoLink;

  UserDto({
    required this.id,
    required this.email,
    required this.userTypeId,
    required this.userType,
    required this.accessLevel,
    required this.studentCode,
    required this.photoLink,
  });

  factory UserDto.fromJson(Map<String, dynamic> json) {
    return UserDto(
      id: json['id'] ?? '',
      email: json['email'] ?? '',
      userTypeId: json['userTypeId'] ?? '',
      userType: json['userType'] ?? '',
      accessLevel: json['accessLevel'] ?? 0,
      studentCode: json['studentCode'] ?? '',
      photoLink: json['photoLink'] ?? '',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'email': email,
      'userTypeId': userTypeId,
      'userType': userType,
      'accessLevel': accessLevel,
      'studentCode': studentCode,
      'photoLink': photoLink,
    };
  }
}

