class SchoolDto {
  final String id;
  final String name;
  final String shortName;
  final String domain;
  final String studentCodePattern;

  SchoolDto({
    required this.id,
    required this.name,
    required this.shortName,
    required this.domain,
    required this.studentCodePattern,
  });

  factory SchoolDto.fromJson(Map<String, dynamic> json) {
    return SchoolDto(
      id: json['id'] ?? '',
      name: json['name'] ?? '',
      shortName: json['shortName'] ?? '',
      domain: json['domain'] ?? '',
      studentCodePattern: json['studentCodePattern'] ?? '',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'shortName': shortName,
      'domain': domain,
      'studentCodePattern': studentCodePattern,
    };
  }
}