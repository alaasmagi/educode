import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:mobile/controllers/create_account_controller.dart';
import 'package:mobile/models/Responses/school_dto.dart';
import 'package:mobile/models/result.dart';
import 'package:mobile/services/loading_manager.dart';
import 'package:mobile/widgets/app_logo.dart';
import 'package:mobile/widgets/email_with_domain_field.dart';
import 'package:mobile/widgets/form_text_field.dart';
import 'package:mobile/widgets/normal_button.dart';
import 'package:mobile/widgets/link_button.dart';
import 'package:mobile/widgets/school_dropdown.dart';
import 'package:mobile/validators/password_validator.dart';

class CreateAccountView extends ConsumerStatefulWidget {
  const CreateAccountView({Key? key}) : super(key: key);

  @override
  ConsumerState<CreateAccountView> createState() => _CreateAccountViewState();
}

class _CreateAccountViewState extends ConsumerState<CreateAccountView> {
  final _formKey = GlobalKey<FormState>();
  final _passwordController = TextEditingController();

  int _currentStep = 0;
  List<SchoolDto> _schools = [];
  SchoolDto? _selectedSchool;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _loadSchools();
    });
  }

  @override
  void dispose() {
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _loadSchools() async {
    final controller = ref.read(createAccountControllerProvider.notifier);
    final result = await LoadingManager.withLoading(ref, () async {
      return await controller.fetchSchools();
    });

    if (!mounted) return;

    if (result is Success<List<SchoolDto>>) {
      setState(() {
        _schools = result.data;
      });
    } else if (result is Failure<List<SchoolDto>>) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Koolide laadimine ebaõnnestus: ${result.error.message}'),
          backgroundColor: Colors.red,
        ),
      );
    }
  }

  void _nextStep() async {
    final form = _formKey.currentState;
    if (form == null) return;
    if (!form.validate()) return;
    form.save();

    if (_currentStep < 2) {
      setState(() {
        _currentStep++;
        _formKey.currentState?.reset();
      });
    } else if (_currentStep == 2) {
      await _submitRegistration();
    }
  }

  void _previousStep() {
    if (_currentStep > 0) {
      setState(() => _currentStep--);
    }
  }

  Future<void> _submitRegistration() async {
    final controller = ref.read(createAccountControllerProvider.notifier);

    final result = await LoadingManager.withLoading(ref, () async {
      return await controller.submitRegistration();
    });

    if (!mounted) return;

    if (result is Success) {
      // Registration successful, navigate to OTP verification page
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Registreerimine õnnestus! Suuname sind kinnituskoodide lehele.'),
          backgroundColor: Colors.green,
        ),
      );

      controller.reset();

      // Navigate to OTP verification page
      context.go('/otp-verification?email=${Uri.encodeComponent(controller.fullEmail)}&fullName=${Uri.encodeComponent(controller.fullName)}&isPostLogin=false');
    } else if (result is Failure) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Registreerimine ebaõnnestus: ${result.error.message}'),
          backgroundColor: Colors.red,
        ),
      );
    }
  }

  void _navigateToLogin() {
    context.pop();
  }

  String _getStepTitle() {
    switch (_currentStep) {
      case 0:
        return 'Kool ja nimi';
      case 1:
        return 'E-post ja üliõpilaskood';
      case 2:
        return 'Salasõna';
      default:
        return '';
    }
  }

  String _getButtonLabel() {
    switch (_currentStep) {
      case 0:
      case 1:
        return 'Jätka';
      case 2:
        return 'Loo konto';
      default:
        return 'Jätka';
    }
  }

  @override
  Widget build(BuildContext context) {
    final controller = ref.read(createAccountControllerProvider.notifier);

    return Scaffold(
      body: SafeArea(
        child: Column(
          children: [
            Expanded(
              child: SingleChildScrollView(
                padding: const EdgeInsets.all(16),
                child: Column(
                  children: [
                    const AppLogo(height: 150),
                    const SizedBox(height: 24),
                    Text(
                      _getStepTitle(),
                      style: Theme.of(context).textTheme.headlineSmall,
                    ),
                    const SizedBox(height: 8),
                    Text(
                      'Samm ${_currentStep + 1} / 3',
                      style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                            color: Colors.grey,
                          ),
                    ),
                    const SizedBox(height: 24),
                    Form(
                      key: _formKey,
                      child: Column(
                        children: [
                          // Step 0: School and Full Name
                          if (_currentStep == 0) ...[
                            SchoolDropdown(
                              schools: _schools,
                              selectedSchool: _selectedSchool,
                              onChanged: (school) {
                                setState(() => _selectedSchool = school);
                                if (school != null) {
                                  controller.setSchool(school);
                                }
                              },
                              validator: (v) => v == null ? 'Palun vali kool' : null,
                            ),
                            const SizedBox(height: 12),
                            FormTextField(
                              fieldKey: const Key('fullName'),
                              label: 'Täisnimi',
                              placeHolder: 'Sisesta oma ees- ja perekonnanimi',
                              keyboardType: TextInputType.name,
                              validator: (v) => (v?.isEmpty ?? true) ? 'Täisnimi on kohustuslik' : null,
                              onSaved: (v) => controller.setFullName(v ?? ''),
                            ),
                          ],
                          // Step 1: Email and Student Code
                          if (_currentStep == 1) ...[
                            EmailWithDomainField(
                              domain: controller.schoolDomain,
                              validator: (v) {
                                if (v == null || v.isEmpty) {
                                  return 'Sisesta e-posti aadressi esimene osa';
                                }
                                if (v.contains('@')) {
                                  return 'Ära sisesta @ märki';
                                }
                                return null;
                              },
                              onSaved: (v) => controller.setEmailLocalPart(v ?? ''),
                            ),
                            const SizedBox(height: 12),
                            FormTextField(
                              fieldKey: const Key('studentCode'),
                              label: 'Üliõpilaskood',
                              placeHolder: 'Sisesta oma üliõpilaskood',
                              keyboardType: TextInputType.text,
                              validator: (v) => (v?.isEmpty ?? true) ? 'Üliõpilaskood on kohustuslik' : null,
                              onSaved: (v) => controller.setStudentCode(v ?? ''),
                            ),
                          ],
                          // Step 2: Password
                          if (_currentStep == 2) ...[
                            TextFormField(
                              key: const Key('password'),
                              controller: _passwordController,
                              decoration: const InputDecoration(
                                labelText: 'Salasõna',
                                hintText: 'Vähemalt 8 tähemärki',
                              ),
                              obscureText: true,
                              validator: PasswordValidator.validate,
                              onSaved: (v) => controller.setPassword(v ?? ''),
                            ),
                            const SizedBox(height: 12),
                            FormTextField(
                              fieldKey: const Key('confirmPassword'),
                              label: 'Kinnita salasõna',
                              placeHolder: 'Sisesta salasõna uuesti',
                              obscureText: true,
                              validator: (v) {
                                if (v == null || v.isEmpty) {
                                  return 'Kinnita salasõna';
                                }
                                if (v != _passwordController.text) {
                                  return 'Salasõnad ei ühti';
                                }
                                return null;
                              },
                              onSaved: (_) {},
                            ),
                          ],
                          const SizedBox(height: 24),
                          Row(
                            children: [
                              if (_currentStep > 0)
                                Expanded(
                                  child: NormalButton(
                                    onPressed: _previousStep,
                                    label: 'Tagasi',
                                    style: NormalButtonStyle.secondary,
                                  ),
                                ),
                              if (_currentStep > 0) const SizedBox(width: 12),
                              Expanded(
                                child: NormalButton(
                                  onPressed: _nextStep,
                                  label: _getButtonLabel(),
                                  style: NormalButtonStyle.primary,
                                ),
                              ),
                            ],
                          ),
                          if (_currentStep == 0)
                            LinkButton(
                              text: 'Konto olemas? Logi sisse!',
                              onPressed: _navigateToLogin,
                            ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

