import 'package:mobile/services/api/api_client.dart';

class GeneralService {
  final ApiClient _apiClient;
  GeneralService(this._apiClient);

  Future<bool> healthCheck() async {
    try {
      final response = await _apiClient.get('General/HealthCheck');

      return response.statusCode != null &&
             response.statusCode! >= 200 &&
             response.statusCode! < 300;
    } catch (e) {
      return false;
    }
  }
}

