import 'dart:convert';
import 'package:dio/dio.dart';
import 'package:mobile/models/Requests/refresh_tokens.dart';
import 'package:mobile/models/Responses/tokens.dart';
import 'package:mobile/services/secure_store.dart';

/// Interceptor that automatically refreshes tokens on 401/403 errors
/// and retries the original request once
class TokenRefreshInterceptor extends Interceptor {
  final Dio dio;
  final SecureStore secureStore;
  final Function()? onRefreshFailed;

  bool _isRefreshing = false;

  TokenRefreshInterceptor({
    required this.dio,
    required this.secureStore,
    this.onRefreshFailed,
  });

  @override
  void onResponse(Response response, ResponseInterceptorHandler handler) async {
    if ((response.statusCode == 401 || response.statusCode == 403) &&
        response.requestOptions.extra['retry'] != true) {

      final refreshed = await _refreshToken();

      if (refreshed) {
        try {
          final options = response.requestOptions;
          options.extra['retry'] = true; // Mark as retry to prevent infinite loop

          // Get the new token and update authorization header
          final tokensJson = await secureStore.get('user_tokens');
          if (tokensJson != null) {
            final tokens = Tokens.fromJson(jsonDecode(tokensJson));
            options.headers['Authorization'] = 'Bearer ${tokens.accessToken}';
          }

          final retryResponse = await dio.fetch(options);
          return handler.resolve(retryResponse);
        } catch (e) {
          return handler.next(response);
        }
      } else {
        // Refresh failed, call the callback if provided
        onRefreshFailed?.call();
        return handler.next(response);
      }
    }

    return handler.next(response);
  }

  Future<bool> _refreshToken() async {
    if (_isRefreshing) {
      // Already refreshing, wait a bit
      await Future.delayed(const Duration(milliseconds: 500));
      return false;
    }

    _isRefreshing = true;

    try {
      final tokensJson = await secureStore.get('user_tokens');
      if (tokensJson == null) {
        return false;
      }

      final tokens = Tokens.fromJson(jsonDecode(tokensJson));

      final request = RefreshTokensRequest(
        clientApp: 'educode-mobile',
        accessToken: tokens.accessToken,
        refreshToken: tokens.refreshToken,
      );

      final response = await dio.post('Auth/Refresh', data: request.toJson());

      if (response.statusCode == 200) {
        final newTokens = Tokens.fromJson(response.data);
        await secureStore.delete('user_tokens');
        await secureStore.save('user_tokens', jsonEncode(newTokens.toJson()));
        return true;
      }

      return false;
    } catch (e) {
      print('Token refresh error: $e');
      return false;
    } finally {
      _isRefreshing = false;
    }
  }
}

