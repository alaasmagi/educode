import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:mobile/widgets/app_logo.dart';
import 'package:mobile/widgets/form_text_field.dart';
import 'package:mobile/widgets/language_switcher.dart';
import 'package:mobile/widgets/normal_button.dart';
import 'package:mobile/widgets/link_button.dart';
import 'package:mobile/validators/email_validator.dart';
import 'package:mobile/validators/password_validator.dart';
import 'package:mobile/widgets/section_divider.dart';
import 'package:mobile/widgets/theme_toggle.dart';

import '../providers/api_providers.dart';
import '../controllers/login_controller.dart';
import '../models/result.dart';
import '../services/loading_manager.dart';

class LoginView extends ConsumerStatefulWidget {
  const LoginView({Key? key}) : super(key: key);

  @override
  ConsumerState<LoginView> createState() => _LoginViewState();
}

class _LoginViewState extends ConsumerState<LoginView> {
  final _formKey = GlobalKey<FormState>();


  @override
  Widget build(BuildContext context) {
    final apiStatusAsync = ref.watch(isAppOnlineProvider);
    final apiStatus = apiStatusAsync.when(
      data: (value) => value,
      loading: () => false,
      error: (_, _) => false,
    );
    final controller = ref.read(loginControllerProvider.notifier);

    void submitLogin() async {
      final form = _formKey.currentState;
      if (form == null) return;
      if (!form.validate()) return;
      form.save();

      final result = await LoadingManager.withLoading(ref, () async {
        return await controller.submitLogin();
      });

      if (!mounted) return;

      if (result is Failure) {
        if (result.error.code != 'user-not-verified') {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text(result.error.message),
              backgroundColor: Colors.red,
            ),
          );
        }
      }
    }

    void submitOfflineMode() {
      final form = _formKey.currentState;
      if (form == null) return;
      if (!form.validate()) return;
      form.save();
      controller.submitOfflineMode();
    }

    void navigateToCreateAccount() {
      context.push('/create-account');
    }

    return Scaffold(
      body: Center(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(16),
          child: Column(
            children: [
              const AppLogo(height: 100),
              LanguageSwitcher(),
              Padding(
                padding: const EdgeInsets.all(16),
                child: Form(
                  key: _formKey,
                  child: Column(
                    children: [
                      Padding(padding: const EdgeInsetsGeometry.fromLTRB(0, 50, 0, 0)),
                      FormTextField(
                        fieldKey: const Key('email'),
                        label: 'E-posti aadress',
                        isEnabled: apiStatus,
                        keyboardType: TextInputType.emailAddress,
                        validator: EmailValidator.validate,
                        onSaved: (v) => controller.setEmail(v ?? ''),
                      ),
                      const SizedBox(height: 12),
                      FormTextField(
                        fieldKey: const Key('password'),
                        label: 'Salasõna',
                        isEnabled: apiStatus,
                        obscureText: true,
                        validator: PasswordValidator.validate,
                        onSaved: (v) => controller.setPassword(v ?? ''),
                      ),
                      const SizedBox(height: 16),
                      NormalButton(
                        onPressed: submitLogin,
                        isEnabled: apiStatus,
                        label: 'Logi sisse',
                        style: NormalButtonStyle.primary,
                      ),
                      LinkButton(
                        text: 'Ei ole veel kontot? Registreeru tudengina!',
                        isEnabled: apiStatus,
                        onPressed: navigateToCreateAccount,
                      ),
                      const SizedBox(height: 60),
                      SectionDivider(label: "Või kasuta ainult offline-režiimi"),
                      const SizedBox(height: 16),
                      FormTextField(
                        fieldKey: const Key('studentCode'),
                        label: 'Üliõpilaskood',
                        placeHolder: "nt. 123456ABCD",
                        onSaved: (v) => controller.setStudentCode(v ?? ''),
                      ),
                      const SizedBox(height: 12),
                      FormTextField(
                        fieldKey: const Key('fullName'),
                        label: 'Täisnimi',
                        placeHolder: "nt. Andres Tamm",
                        onSaved: (v) => controller.setFullName(v ?? ''),
                      ),
                      const SizedBox(height: 16),
                      NormalButton(
                        onPressed: submitOfflineMode,
                        label: 'Jätka',
                        style: NormalButtonStyle.secondary,
                      ),
                    ],
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

