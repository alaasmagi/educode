import 'package:mobile/models/Responses/api_error.dart';

sealed class ApiResponse<T> {
  const ApiResponse();
}

class Success<T> extends ApiResponse<T> {
  final T data;
  const Success(this.data);
}

class Failure<T> extends ApiResponse<T> {
  final ApiError error;
  const Failure(this.error);
}