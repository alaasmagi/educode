import 'package:mobile/models/Responses/api_error.dart';
import 'package:mobile/models/Responses/school_dto.dart';
import 'package:mobile/models/result.dart';
import 'package:mobile/services/api/api_client.dart';

class SchoolService {
  final ApiClient _apiClient;
  SchoolService(this._apiClient);

  Future<ApiResponse<List<SchoolDto>>> getAllSchools() async {
    final response = await _apiClient.get('School');

    if (response.statusCode == 200) {
      final List<dynamic> data = response.data as List<dynamic>;
      final schools = data.map((json) => SchoolDto.fromJson(json)).toList();
      return Success(schools);
    }

    return Failure(ApiError.fromJson(response.data));
  }
}

