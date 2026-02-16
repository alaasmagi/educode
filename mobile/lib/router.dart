import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:mobile/views/auth_gate_view.dart';
import 'package:mobile/views/login_view.dart';
import 'package:mobile/views/create_account_view.dart';
import 'package:mobile/views/otp_verification_view.dart';
import 'package:mobile/views/role_selection_view.dart';
import 'package:mobile/views/student_home_view.dart';
import 'package:mobile/views/teacher_home_view.dart';

final GoRouter router = GoRouter(
  initialLocation: '/',
  routes: <RouteBase>[
    // Auth Gate - kontrollb login state'i ja suunab õigele lehele
    GoRoute(
      path: '/',
      builder: (BuildContext context, GoRouterState state) {
        return const AuthGateView();
      },
    ),
    // Login
    GoRoute(
      path: '/login',
      builder: (BuildContext context, GoRouterState state) {
        return const LoginView();
      },
    ),
    // Create Account
    GoRoute(
      path: '/create-account',
      builder: (BuildContext context, GoRouterState state) {
        return const CreateAccountView();
      },
    ),
    // OTP Verification
    GoRoute(
      path: '/otp-verification',
      builder: (BuildContext context, GoRouterState state) {
        final email = state.uri.queryParameters['email'] ?? '';
        final fullName = state.uri.queryParameters['fullName'] ?? '';
        final isPostLogin = state.uri.queryParameters['isPostLogin'] == 'true';
        return OtpVerificationView(
          email: email,
          fullName: fullName,
          isPostLogin: isPostLogin,
        );
      },
    ),
    // Role Selection - accessLevel 2 või 4+
    GoRoute(
      path: '/role-selection',
      builder: (BuildContext context, GoRouterState state) {
        return const RoleSelectionView();
      },
    ),
    // Student Home - accessLevel 1
    GoRoute(
      path: '/student-home',
      builder: (BuildContext context, GoRouterState state) {
        return const StudentHomeView();
      },
    ),
    // Teacher Home - accessLevel 3+
    GoRoute(
      path: '/teacher-home',
      builder: (BuildContext context, GoRouterState state) {
        return const TeacherHomeView();
      },
    ),
  ],
);
