import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:mobile/controllers/otp_controller.dart';
import 'package:mobile/models/result.dart';
import 'package:mobile/services/loading_manager.dart';
import 'package:mobile/widgets/app_logo.dart';
import 'package:mobile/widgets/form_text_field.dart';
import 'package:mobile/widgets/normal_button.dart';
import 'package:mobile/widgets/link_button.dart';


class OtpVerificationView extends ConsumerStatefulWidget {
  final String email;
  final String fullName;
  final bool isPostLogin;

  const OtpVerificationView({
    Key? key,
    required this.email,
    required this.fullName,
    this.isPostLogin = false,
  }) : super(key: key);

  @override
  ConsumerState<OtpVerificationView> createState() => _OtpVerificationViewState();
}

class _OtpVerificationViewState extends ConsumerState<OtpVerificationView> {
  final _formKey = GlobalKey<FormState>();
  bool _otpSent = false;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _initializeController();
      if (widget.isPostLogin) {
        // If coming from login, automatically request OTP
        _requestOtp();
      }
    });
  }

  void _initializeController() {
    final controller = ref.read(otpControllerProvider.notifier);
    controller.setEmail(widget.email);
    controller.setFullName(widget.fullName);
  }

  Future<void> _requestOtp() async {
    final controller = ref.read(otpControllerProvider.notifier);

    final result = await LoadingManager.withLoading(ref, () async {
      return await controller.requestOtp();
    });

    if (!mounted) return;

    if (result is Success) {
      setState(() {
        _otpSent = true;
      });
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Kinnituskood saadetud aadressile ${widget.email}'),
          backgroundColor: Colors.green,
        ),
      );
    } else if (result is Failure) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('OTP saatmine ebaõnnestus: ${result.error.message}'),
          backgroundColor: Colors.red,
        ),
      );
    }
  }

  Future<void> _verifyOtp() async {
    final form = _formKey.currentState;
    if (form == null) return;
    if (!form.validate()) return;
    form.save();

    final controller = ref.read(otpControllerProvider.notifier);

    final result = await LoadingManager.withLoading(ref, () async {
      return await controller.verifyOtp();
    });

    if (!mounted) return;

    if (result is Success) {
      final message = widget.isPostLogin
          ? 'OTP kinnitatud! Palun logi uuesti sisse.'
          : 'Konto loodud edukalt! Palun logi sisse.';

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(message),
          backgroundColor: Colors.green,
        ),
      );
      controller.reset();
      context.go('/login');
    } else if (result is Failure) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('OTP kinnitamine ebaõnnestus: ${result.error.message}'),
          backgroundColor: Colors.red,
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Kinnita konto'),
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: () => context.go('/login'),
        ),
      ),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(16),
          child: Column(
            children: [
              const AppLogo(height: 150),
              const SizedBox(height: 24),
              Text(
                'OTP Kinnitamine',
                style: Theme.of(context).textTheme.headlineSmall,
              ),
              const SizedBox(height: 24),
              if (!_otpSent) ...[
                Text(
                  'Vajutage nuppu, et saata kinnituskood e-posti aadressile:',
                  style: Theme.of(context).textTheme.bodyMedium,
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 8),
                Text(
                  widget.email,
                  style: Theme.of(context).textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 24),
                NormalButton(
                  onPressed: _requestOtp,
                  label: 'Saada kinnituskood',
                  style: NormalButtonStyle.primary,
                ),
              ] else ...[
                Text(
                  'Saatsime kinnituskoodi e-posti aadressile:',
                  style: Theme.of(context).textTheme.bodyMedium,
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 8),
                Text(
                  widget.email,
                  style: Theme.of(context).textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 24),
                Form(
                  key: _formKey,
                  child: Column(
                    children: [
                      FormTextField(
                        fieldKey: const Key('otp'),
                        label: 'Kinnituskood',
                        placeHolder: 'nt. 123456',
                        keyboardType: TextInputType.number,
                        validator: (v) {
                          if (v == null || v.isEmpty) {
                            return 'Sisesta kinnituskood';
                          }
                          return null;
                        },
                        onSaved: (v) => ref.read(otpControllerProvider.notifier).setOtp(v ?? ''),
                      ),
                      const SizedBox(height: 24),
                      NormalButton(
                        onPressed: _verifyOtp,
                        label: 'Kinnita',
                        style: NormalButtonStyle.primary,
                      ),
                      const SizedBox(height: 12),
                      LinkButton(
                        text: 'Saada uuesti kinnituskood',
                        onPressed: _requestOtp,
                      ),
                    ],
                  ),
                ),
              ],
            ],
          ),
        ),
      ),
    );
  }
}

